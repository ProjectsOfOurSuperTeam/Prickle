using System.Net.Http.Json;
using Bogus;
using Prickle.Api.Endpoints;
using Prickle.Api.Endpoints.Soil.Types;
using Prickle.Application.Soil.Types;

namespace Prickle.Integration.Tests.Api.SoilTypeEndpointTests;

[Collection(nameof(ApiTestsCollectionMarker))]
public sealed class AddSoilTypeEndpointTest : ApiBaseIntegrationTest
{
    private readonly Faker _faker;

    public AddSoilTypeEndpointTest(AppHostFixture appHostFactory) : base(appHostFactory)
    {
        _faker = new Faker();
    }

    [Fact]
    public async Task AddSoilType_ValidRequest_ShouldReturnCreatedWithSoilType()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");
        var newSoilType = new Add.AddSoilTypeRequest(_faker.Lorem.Word());

        // Act
        var response = await client.PostAsJsonAsync(ApiEndpoints.Soil.Types.Add, newSoilType, cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        var soilTypeResponse = await response.Content.ReadFromJsonAsync<SoilTypeResponse>(cancellationToken: cts);
        soilTypeResponse.ShouldNotBeNull();
        soilTypeResponse.Id.ShouldBeGreaterThan(0);
        soilTypeResponse.Name.ShouldBe(newSoilType.Name);
        response.Headers.Location.ShouldNotBeNull();
    }

    [Fact]
    public async Task AddSoilType_EmptyName_ShouldReturnBadRequest()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");
        var newSoilType = new Add.AddSoilTypeRequest(string.Empty);

        // Act
        var response = await client.PostAsJsonAsync(ApiEndpoints.Soil.Types.Add, newSoilType, cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task AddSoilType_NameTooLong_ShouldReturnBadRequest()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");
        var longName = _faker.Random.String2(256); // Max is 255
        var newSoilType = new Add.AddSoilTypeRequest(longName);

        // Act
        var response = await client.PostAsJsonAsync(ApiEndpoints.Soil.Types.Add, newSoilType, cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task AddSoilType_DuplicateName_ShouldReturnBadRequest()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");
        var soilTypeName = $"{_faker.Lorem.Word()}-{Guid.NewGuid():N}";
        var newSoilType = new Add.AddSoilTypeRequest(soilTypeName);

        // Act - Add the soil type first time
        var firstResponse = await client.PostAsJsonAsync(ApiEndpoints.Soil.Types.Add, newSoilType, cts);
        firstResponse.StatusCode.ShouldBe(HttpStatusCode.Created);

        // Act - Try to add the same soil type again
        var secondResponse = await client.PostAsJsonAsync(ApiEndpoints.Soil.Types.Add, newSoilType, cts);

        // Assert
        secondResponse.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task AddSoilType_DuplicateNameDifferentCasing_ShouldReturnBadRequest()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");
        var baseTypeName = $"{_faker.Lorem.Word()}-{Guid.NewGuid():N}";
        var soilTypeName = baseTypeName.ToLowerInvariant();
        var firstRequest = new Add.AddSoilTypeRequest(soilTypeName);
        var secondRequest = new Add.AddSoilTypeRequest(soilTypeName.ToUpperInvariant());

        // Act - Add the soil type with lowercase
        var firstResponse = await client.PostAsJsonAsync(ApiEndpoints.Soil.Types.Add, firstRequest, cts);
        firstResponse.StatusCode.ShouldBe(HttpStatusCode.Created);

        // Act - Try to add with uppercase
        var secondResponse = await client.PostAsJsonAsync(ApiEndpoints.Soil.Types.Add, secondRequest, cts);

        // Assert
        secondResponse.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task AddSoilType_NameWithWhitespace_ShouldTrimAndReturnCreated()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");
        var soilTypeName = $"{_faker.Lorem.Word()}-{Guid.NewGuid():N}";
        var newSoilType = new Add.AddSoilTypeRequest($"  {soilTypeName}  ");

        // Act
        var response = await client.PostAsJsonAsync(ApiEndpoints.Soil.Types.Add, newSoilType, cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        var soilTypeResponse = await response.Content.ReadFromJsonAsync<SoilTypeResponse>(cancellationToken: cts);
        soilTypeResponse.ShouldNotBeNull();
        soilTypeResponse.Name.ShouldBe(newSoilType.Name.Trim()); // should be normalized
    }

    [Theory]
    [InlineData("Clay")]
    [InlineData("Sandy Loam")]
    [InlineData("Peat-123")]
    public async Task AddSoilType_VariousValidNames_ShouldReturnCreated(string soilTypeNamePrefix)
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");
        // Make the name unique by adding a GUID suffix to avoid conflicts with other tests
        var soilTypeName = $"{soilTypeNamePrefix}-{Guid.NewGuid():N}";
        var newSoilType = new Add.AddSoilTypeRequest(soilTypeName);

        // Act
        var response = await client.PostAsJsonAsync(ApiEndpoints.Soil.Types.Add, newSoilType, cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        var soilTypeResponse = await response.Content.ReadFromJsonAsync<SoilTypeResponse>(cancellationToken: cts);
        soilTypeResponse.ShouldNotBeNull();
        soilTypeResponse.Name.ShouldBe(soilTypeName);
    }
}