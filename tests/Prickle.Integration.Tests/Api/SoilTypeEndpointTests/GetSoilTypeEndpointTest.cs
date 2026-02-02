using System.Net.Http.Json;
using Bogus;
using Prickle.Api.Endpoints;
using Prickle.Api.Endpoints.Soil;
using Prickle.Application.Soil.Types;

namespace Prickle.Integration.Tests.Api.SoilTypeEndpointTests;

[Collection(nameof(ApiTestsCollectionMarker))]
public sealed class GetSoilTypeEndpointTest : ApiBaseIntegrationTest
{
    private readonly Faker _faker;

    public GetSoilTypeEndpointTest(AppHostFixture appHostFactory) : base(appHostFactory)
    {
        _faker = new Faker();
    }

    [Fact]
    public async Task GetSoilType_ExistingId_ShouldReturnOkWithSoilType()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        // Create a soil type first
        var soilTypeName = $"{_faker.Lorem.Word()}-{Guid.NewGuid():N}";
        var createRequest = new Add.AddSoilTypeRequest(soilTypeName);
        var createResponse = await client.PostAsJsonAsync(ApiEndpoints.Soil.Add, createRequest, cts);
        var createdSoilType = await createResponse.Content.ReadFromJsonAsync<SoilTypeResponse>(cancellationToken: cts);

        // Act
        var url = ApiEndpoints.Soil.Get.Replace("{id:int}", createdSoilType!.Id.ToString());
        var response = await client.GetAsync(url, cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var soilTypeResponse = await response.Content.ReadFromJsonAsync<SoilTypeResponse>(cancellationToken: cts);
        soilTypeResponse.ShouldNotBeNull();
        soilTypeResponse.Id.ShouldBe(createdSoilType.Id);
        soilTypeResponse.Name.ShouldBe(soilTypeName);
    }

    [Fact]
    public async Task GetSoilType_NonExistentId_ShouldReturnBadRequest()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");
        var nonExistentId = _faker.Random.Int(999999, 9999999);

        // Act
        var url = ApiEndpoints.Soil.Get.Replace("{id:int}", nonExistentId.ToString());
        var response = await client.GetAsync(url, cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public async Task GetSoilType_InvalidId_ShouldReturnBadRequest(int invalidId)
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        // Act
        var url = ApiEndpoints.Soil.Get.Replace("{id:int}", invalidId.ToString());
        var response = await client.GetAsync(url, cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetSoilType_MultipleRequests_ShouldReturnConsistentResults()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        // Create a soil type
        var soilTypeName = $"{_faker.Lorem.Word()}-{Guid.NewGuid():N}";
        var createRequest = new Add.AddSoilTypeRequest(soilTypeName);
        var createResponse = await client.PostAsJsonAsync(ApiEndpoints.Soil.Add, createRequest, cts);
        var createdSoilType = await createResponse.Content.ReadFromJsonAsync<SoilTypeResponse>(cancellationToken: cts);

        // Act - Make multiple GET requests
        var url = ApiEndpoints.Soil.Get.Replace("{id:int}", createdSoilType!.Id.ToString());
        var firstResponse = await client.GetAsync(url, cts);
        var secondResponse = await client.GetAsync(url, cts);

        // Assert
        firstResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        secondResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        var firstSoilType = await firstResponse.Content.ReadFromJsonAsync<SoilTypeResponse>(cancellationToken: cts);
        var secondSoilType = await secondResponse.Content.ReadFromJsonAsync<SoilTypeResponse>(cancellationToken: cts);

        firstSoilType.ShouldNotBeNull();
        secondSoilType.ShouldNotBeNull();
        firstSoilType.Id.ShouldBe(secondSoilType.Id);
        firstSoilType.Name.ShouldBe(secondSoilType.Name);
    }

    [Fact]
    public async Task GetSoilType_AfterCreation_ShouldReturnCreatedData()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");
        var soilTypeName = $"{_faker.Lorem.Word()}-{Guid.NewGuid():N}";

        // Act - Create and immediately retrieve
        var createRequest = new Add.AddSoilTypeRequest(soilTypeName);
        var createResponse = await client.PostAsJsonAsync(ApiEndpoints.Soil.Add, createRequest, cts);
        var createdSoilType = await createResponse.Content.ReadFromJsonAsync<SoilTypeResponse>(cancellationToken: cts);

        var url = ApiEndpoints.Soil.Get.Replace("{id:int}", createdSoilType!.Id.ToString());
        var getResponse = await client.GetAsync(url, cts);

        // Assert
        getResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var retrievedSoilType = await getResponse.Content.ReadFromJsonAsync<SoilTypeResponse>(cancellationToken: cts);
        retrievedSoilType.ShouldNotBeNull();
        retrievedSoilType.Id.ShouldBe(createdSoilType.Id);
        retrievedSoilType.Name.ShouldBe(soilTypeName);
    }

    [Fact]
    public async Task GetSoilType_DifferentIds_ShouldReturnDifferentSoilTypes()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        // Create two different soil types
        var firstSoilTypeName = $"{_faker.Lorem.Word()}-{Guid.NewGuid():N}";
        var secondSoilTypeName = $"{_faker.Lorem.Word()}-{Guid.NewGuid():N}";

        var firstCreateResponse = await client.PostAsJsonAsync(ApiEndpoints.Soil.Add,
            new Add.AddSoilTypeRequest(firstSoilTypeName), cts);
        var firstCreatedSoilType = await firstCreateResponse.Content.ReadFromJsonAsync<SoilTypeResponse>(cancellationToken: cts);

        var secondCreateResponse = await client.PostAsJsonAsync(ApiEndpoints.Soil.Add,
            new Add.AddSoilTypeRequest(secondSoilTypeName), cts);
        var secondCreatedSoilType = await secondCreateResponse.Content.ReadFromJsonAsync<SoilTypeResponse>(cancellationToken: cts);

        // Act
        var firstUrl = ApiEndpoints.Soil.Get.Replace("{id:int}", firstCreatedSoilType!.Id.ToString());
        var secondUrl = ApiEndpoints.Soil.Get.Replace("{id:int}", secondCreatedSoilType!.Id.ToString());

        var firstGetResponse = await client.GetAsync(firstUrl, cts);
        var secondGetResponse = await client.GetAsync(secondUrl, cts);

        // Assert
        firstGetResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        secondGetResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        var firstRetrievedSoilType = await firstGetResponse.Content.ReadFromJsonAsync<SoilTypeResponse>(cancellationToken: cts);
        var secondRetrievedSoilType = await secondGetResponse.Content.ReadFromJsonAsync<SoilTypeResponse>(cancellationToken: cts);

        firstRetrievedSoilType.ShouldNotBeNull();
        secondRetrievedSoilType.ShouldNotBeNull();

        firstRetrievedSoilType.Id.ShouldNotBe(secondRetrievedSoilType.Id);
        firstRetrievedSoilType.Name.ShouldBe(firstSoilTypeName);
        secondRetrievedSoilType.Name.ShouldBe(secondSoilTypeName);
    }
}