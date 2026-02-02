using System.Net.Http.Json;
using Bogus;
using Prickle.Api.Endpoints;
using Prickle.Api.Endpoints.Soil.Types;
using Prickle.Application.Soil.Types;

namespace Prickle.Integration.Tests.Api.SoilTypeEndpointTests;

[Collection(nameof(ApiTestsCollectionMarker))]
public sealed class UpdateSoilTypeEndpointTest : ApiBaseIntegrationTest
{
    private readonly Faker _faker;
    private readonly ITestOutputHelper _outputHelper;

    public UpdateSoilTypeEndpointTest(AppHostFixture appHostFactory, ITestOutputHelper outputHelper) : base(appHostFactory)
    {
        _faker = new Faker();
        _outputHelper = outputHelper;
    }

    [Fact]
    public async Task UpdateSoilType_ValidRequest_ShouldReturnOkWithUpdatedSoilType()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        // Create a soil type first
        var originalName = $"{_faker.Lorem.Word()}-{Guid.NewGuid():N}";
        var createRequest = new Add.AddSoilTypeRequest(originalName);
        var createResponse = await client.PostAsJsonAsync(ApiEndpoints.Soil.Types.Add, createRequest, cts);
        var createdSoilType = await createResponse.Content.ReadFromJsonAsync<SoilTypeResponse>(cancellationToken: cts);

        // Act
        var newName = $"{_faker.Lorem.Word()}-{Guid.NewGuid():N}";
        var updateRequest = new Update.UpdateSoilTypeRequest(newName);
        var url = ApiEndpoints.Soil.Types.Update.Replace("{id:int}", createdSoilType!.Id.ToString());
        var response = await client.PatchAsJsonAsync(url, updateRequest, cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var updatedSoilType = await response.Content.ReadFromJsonAsync<SoilTypeResponse>(cancellationToken: cts);
        updatedSoilType.ShouldNotBeNull();
        updatedSoilType.Id.ShouldBe(createdSoilType.Id);
        updatedSoilType.Name.ShouldBe(newName);
    }

    [Fact]
    public async Task UpdateSoilType_NonExistentId_ShouldReturnBadRequest()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");
        var nonExistentId = _faker.Random.Int(999999, 9999999);

        // Act
        var updateRequest = new Update.UpdateSoilTypeRequest($"{_faker.Lorem.Word()}-{Guid.NewGuid():N}");
        var url = ApiEndpoints.Soil.Types.Update.Replace("{id:int}", nonExistentId.ToString());
        var response = await client.PatchAsJsonAsync(url, updateRequest, cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public async Task UpdateSoilType_InvalidId_ShouldReturnBadRequest(int invalidId)
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        // Act
        var updateRequest = new Update.UpdateSoilTypeRequest($"{_faker.Lorem.Word()}-{Guid.NewGuid():N}");
        var url = ApiEndpoints.Soil.Types.Update.Replace("{id:int}", invalidId.ToString());
        var response = await client.PatchAsJsonAsync(url, updateRequest, cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateSoilType_EmptyName_ShouldReturnBadRequest()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        // Create a soil type first
        var createRequest = new Add.AddSoilTypeRequest($"{_faker.Lorem.Word()}-{Guid.NewGuid():N}");
        var createResponse = await client.PostAsJsonAsync(ApiEndpoints.Soil.Types.Add, createRequest, cts);
        var createdSoilType = await createResponse.Content.ReadFromJsonAsync<SoilTypeResponse>(cancellationToken: cts);

        // Act
        var updateRequest = new Update.UpdateSoilTypeRequest(string.Empty);
        var url = ApiEndpoints.Soil.Types.Update.Replace("{id:int}", createdSoilType!.Id.ToString());
        var response = await client.PatchAsJsonAsync(url, updateRequest, cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateSoilType_NameTooLong_ShouldReturnBadRequest()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        // Create a soil type first
        var createRequest = new Add.AddSoilTypeRequest($"{_faker.Lorem.Word()}-{Guid.NewGuid():N}");
        var createResponse = await client.PostAsJsonAsync(ApiEndpoints.Soil.Types.Add, createRequest, cts);
        var createdSoilType = await createResponse.Content.ReadFromJsonAsync<SoilTypeResponse>(cancellationToken: cts);

        // Act
        var longName = _faker.Random.String2(256); // Max is 255
        var updateRequest = new Update.UpdateSoilTypeRequest(longName);
        var url = ApiEndpoints.Soil.Types.Update.Replace("{id:int}", createdSoilType!.Id.ToString());
        var response = await client.PatchAsJsonAsync(url, updateRequest, cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateSoilType_DuplicateName_ShouldReturnBadRequest()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        // Create two soil types
        var firstName = $"{_faker.Lorem.Word()}-{Guid.NewGuid():N}";
        var secondName = $"{_faker.Lorem.Word()}-{Guid.NewGuid():N}";

        var firstCreateResponse = await client.PostAsJsonAsync(ApiEndpoints.Soil.Types.Add,
            new Add.AddSoilTypeRequest(firstName), cts);
        var firstCreatedSoilType = await firstCreateResponse.Content.ReadFromJsonAsync<SoilTypeResponse>(cancellationToken: cts);

        var secondCreateResponse = await client.PostAsJsonAsync(ApiEndpoints.Soil.Types.Add,
            new Add.AddSoilTypeRequest(secondName), cts);
        var secondCreatedSoilType = await secondCreateResponse.Content.ReadFromJsonAsync<SoilTypeResponse>(cancellationToken: cts);

        // Act - Try to update second soil type to first soil type's name
        var updateRequest = new Update.UpdateSoilTypeRequest(firstName);
        var url = ApiEndpoints.Soil.Types.Update.Replace("{id:int}", secondCreatedSoilType!.Id.ToString());
        var response = await client.PatchAsJsonAsync(url, updateRequest, cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateSoilType_DuplicateNameDifferentCasing_ShouldReturnBadRequest()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        // Create two soil types with unique names
        var baseFirstName = $"{_faker.Lorem.Word()}-{Guid.NewGuid():N}";
        var firstName = baseFirstName.ToLowerInvariant();
        var secondName = $"{_faker.Lorem.Word()}-{Guid.NewGuid():N}";

        var firstCreateResponse = await client.PostAsJsonAsync(ApiEndpoints.Soil.Types.Add,
            new Add.AddSoilTypeRequest(firstName), cts);
        var firstCreatedSoilType = await firstCreateResponse.Content.ReadFromJsonAsync<SoilTypeResponse>(cancellationToken: cts);

        var secondCreateResponse = await client.PostAsJsonAsync(ApiEndpoints.Soil.Types.Add,
            new Add.AddSoilTypeRequest(secondName), cts);
        var secondCreatedSoilType = await secondCreateResponse.Content.ReadFromJsonAsync<SoilTypeResponse>(cancellationToken: cts);

        // Act - Try to update second soil type to first soil type's name (different casing)
        var updateRequest = new Update.UpdateSoilTypeRequest(firstName.ToUpperInvariant());
        var url = ApiEndpoints.Soil.Types.Update.Replace("{id:int}", secondCreatedSoilType!.Id.ToString());
        var response = await client.PatchAsJsonAsync(url, updateRequest, cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateSoilType_UpdateToSameName_ShouldReturnOk()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        // Create a soil type
        var originalName = $"{_faker.Lorem.Word()}-{Guid.NewGuid():N}";
        var createRequest = new Add.AddSoilTypeRequest(originalName);
        var createResponse = await client.PostAsJsonAsync(ApiEndpoints.Soil.Types.Add, createRequest, cts);
        var createdSoilType = await createResponse.Content.ReadFromJsonAsync<SoilTypeResponse>(cancellationToken: cts);

        // Act - Update to the same name
        var updateRequest = new Update.UpdateSoilTypeRequest(originalName);
        var url = ApiEndpoints.Soil.Types.Update.Replace("{id:int}", createdSoilType!.Id.ToString());
        var response = await client.PatchAsJsonAsync(url, updateRequest, cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var updatedSoilType = await response.Content.ReadFromJsonAsync<SoilTypeResponse>(cancellationToken: cts);
        updatedSoilType.ShouldNotBeNull();
        updatedSoilType.Name.ShouldBe(originalName);
    }

    [Fact]
    public async Task UpdateSoilType_UpdateWithWhitespace_ShouldReturnOk()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        // Create a soil type
        var createRequest = new Add.AddSoilTypeRequest($"{_faker.Lorem.Word()}-{Guid.NewGuid():N}");
        var createResponse = await client.PostAsJsonAsync(ApiEndpoints.Soil.Types.Add, createRequest, cts);
        var createdSoilType = await createResponse.Content.ReadFromJsonAsync<SoilTypeResponse>(cancellationToken: cts);

        // Act
        var newName = $"{_faker.Lorem.Word()}-{Guid.NewGuid():N}";
        var updateRequest = new Update.UpdateSoilTypeRequest($"  {newName}  ");
        var url = ApiEndpoints.Soil.Types.Update.Replace("{id:int}", createdSoilType!.Id.ToString());
        var response = await client.PatchAsJsonAsync(url, updateRequest, cts);
        var content = await response.Content.ReadAsStringAsync(cts);
        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        _outputHelper.WriteLine(content);
        var updatedSoilType = await response.Content.ReadFromJsonAsync<SoilTypeResponse>(cancellationToken: cts);
        updatedSoilType.ShouldNotBeNull();
    }

    [Fact]
    public async Task UpdateSoilType_MultipleUpdates_ShouldReturnOkForEach()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        // Create a soil type
        var createRequest = new Add.AddSoilTypeRequest($"{_faker.Lorem.Word()}-{Guid.NewGuid():N}");
        var createResponse = await client.PostAsJsonAsync(ApiEndpoints.Soil.Types.Add, createRequest, cts);
        var createdSoilType = await createResponse.Content.ReadFromJsonAsync<SoilTypeResponse>(cancellationToken: cts);

        // Act - Update multiple times
        var firstNewName = $"{_faker.Lorem.Word()}-{Guid.NewGuid():N}";
        var firstUpdateRequest = new Update.UpdateSoilTypeRequest(firstNewName);
        var url = ApiEndpoints.Soil.Types.Update.Replace("{id:int}", createdSoilType!.Id.ToString());
        var firstResponse = await client.PatchAsJsonAsync(url, firstUpdateRequest, cts);

        var secondNewName = $"{_faker.Lorem.Word()}-{Guid.NewGuid():N}";
        var secondUpdateRequest = new Update.UpdateSoilTypeRequest(secondNewName);
        var secondResponse = await client.PatchAsJsonAsync(url, secondUpdateRequest, cts);

        // Assert
        firstResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var firstUpdatedSoilType = await firstResponse.Content.ReadFromJsonAsync<SoilTypeResponse>(cancellationToken: cts);
        firstUpdatedSoilType.ShouldNotBeNull();
        firstUpdatedSoilType.Name.ShouldBe(firstNewName);

        secondResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var secondUpdatedSoilType = await secondResponse.Content.ReadFromJsonAsync<SoilTypeResponse>(cancellationToken: cts);
        secondUpdatedSoilType.ShouldNotBeNull();
        secondUpdatedSoilType.Name.ShouldBe(secondNewName);
    }

    [Fact]
    public async Task UpdateSoilType_VerifyPersistedChange_ShouldReturnUpdatedName()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        // Create a soil type
        var createRequest = new Add.AddSoilTypeRequest($"{_faker.Lorem.Word()}-{Guid.NewGuid():N}");
        var createResponse = await client.PostAsJsonAsync(ApiEndpoints.Soil.Types.Add, createRequest, cts);
        var createdSoilType = await createResponse.Content.ReadFromJsonAsync<SoilTypeResponse>(cancellationToken: cts);

        // Act - Update
        var newName = $"{_faker.Lorem.Word()}-{Guid.NewGuid():N}";
        var updateRequest = new Update.UpdateSoilTypeRequest(newName);
        var updateUrl = ApiEndpoints.Soil.Types.Update.Replace("{id:int}", createdSoilType!.Id.ToString());
        var updateResponse = await client.PatchAsJsonAsync(updateUrl, updateRequest, cts);

        // Act - Get to verify persistence
        var getUrl = ApiEndpoints.Soil.Types.Get.Replace("{id:int}", createdSoilType.Id.ToString());
        var getResponse = await client.GetAsync(getUrl, cts);

        // Assert
        updateResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        getResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        var retrievedSoilType = await getResponse.Content.ReadFromJsonAsync<SoilTypeResponse>(cancellationToken: cts);
        retrievedSoilType.ShouldNotBeNull();
        retrievedSoilType.Name.ShouldBe(newName);
    }

    [Theory]
    [InlineData("Clay")]
    [InlineData("Sandy Loam")]
    [InlineData("Peat")]
    public async Task UpdateSoilType_VariousValidNames_ShouldReturnOk(string namePrefix)
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        // Create a soil type
        var createRequest = new Add.AddSoilTypeRequest($"{_faker.Lorem.Word()}-{Guid.NewGuid():N}");
        var createResponse = await client.PostAsJsonAsync(ApiEndpoints.Soil.Types.Add, createRequest, cts);
        var createdSoilType = await createResponse.Content.ReadFromJsonAsync<SoilTypeResponse>(cancellationToken: cts);

        // Act - Make the name unique by adding a GUID suffix
        var newName = $"{namePrefix}-{Guid.NewGuid():N}";
        var updateRequest = new Update.UpdateSoilTypeRequest(newName);
        var url = ApiEndpoints.Soil.Types.Update.Replace("{id:int}", createdSoilType!.Id.ToString());
        var response = await client.PatchAsJsonAsync(url, updateRequest, cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var updatedSoilType = await response.Content.ReadFromJsonAsync<SoilTypeResponse>(cancellationToken: cts);
        updatedSoilType.ShouldNotBeNull();
        updatedSoilType.Name.ShouldBe(newName);
    }
}