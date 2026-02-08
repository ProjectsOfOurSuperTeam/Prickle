using System.Net.Http.Json;
using Bogus;
using Prickle.Api.Endpoints;
using Prickle.Application.Decorations;
using Prickle.Domain.Decorations;
using DecorationsEndpoints = Prickle.Api.Endpoints.Decorations;

namespace Prickle.Integration.Tests.Api.DecorationsEndpointTests;

[Collection(nameof(ApiTestsCollectionMarker))]
public sealed class GetDecorationEndpointTest : ApiBaseIntegrationTest
{
    private readonly Faker _faker;

    public GetDecorationEndpointTest(AppHostFixture appHostFactory) : base(appHostFactory)
    {
        _faker = new Faker();
    }

    [Fact]
    public async Task GetDecoration_ExistingId_ShouldReturnOkWithDecoration()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        // Create a decoration first
        var decorationName = $"{_faker.Lorem.Word()}-{Guid.NewGuid():N}";
        var createRequest = new DecorationsEndpoints.Add.AddDecorationRequest
        {
            Name = decorationName,
            Description = _faker.Lorem.Sentence(),
            Category = DecorationCategory.Stones.Value,
            ImageUrl = "https://example.com/image.jpg",
            ImageIsometricUrl = "https://example.com/isometric.jpg"
        };
        var createResponse = await client.PostAsJsonAsync(ApiEndpoints.Decorations.Add, createRequest, cts);
        var createdDecoration = await createResponse.Content.ReadFromJsonAsync<DecorationResponse>(cancellationToken: cts);

        // Act
        var url = ApiEndpoints.Decorations.Get.Replace("{id:guid}", createdDecoration!.Id.ToString());
        var response = await client.GetAsync(url, cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var decorationResponse = await response.Content.ReadFromJsonAsync<DecorationResponse>(cancellationToken: cts);
        decorationResponse.ShouldNotBeNull();
        decorationResponse.Id.ShouldBe(createdDecoration.Id);
        decorationResponse.Name.ShouldBe(decorationName);
        decorationResponse.Description.ShouldBe(createRequest.Description);
        decorationResponse.Category.Value.ShouldBe(DecorationCategory.Stones.Value);
        decorationResponse.ImageUrl.ShouldBe(createRequest.ImageUrl);
        decorationResponse.ImageIsometricUrl.ShouldBe(createRequest.ImageIsometricUrl);
    }

    [Fact]
    public async Task GetDecoration_NonExistentId_ShouldReturnBadRequest()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");
        var nonExistentId = Guid.NewGuid();

        // Act
        var url = ApiEndpoints.Decorations.Get.Replace("{id:guid}", nonExistentId.ToString());
        var response = await client.GetAsync(url, cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetDecoration_InvalidGuidFormat_ShouldReturnNotFound()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        // Act
        var url = ApiEndpoints.Decorations.Get.Replace("{id:guid}", "invalid-guid");
        var response = await client.GetAsync(url, cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetDecoration_ReturnsCorrectContentType()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        // Create a decoration first
        var decorationName = $"{_faker.Lorem.Word()}-{Guid.NewGuid():N}";
        var createRequest = new DecorationsEndpoints.Add.AddDecorationRequest
        {
            Name = decorationName,
            Category = DecorationCategory.Sand.Value
        };
        var createResponse = await client.PostAsJsonAsync(ApiEndpoints.Decorations.Add, createRequest, cts);
        var createdDecoration = await createResponse.Content.ReadFromJsonAsync<DecorationResponse>(cancellationToken: cts);

        // Act
        var url = ApiEndpoints.Decorations.Get.Replace("{id:guid}", createdDecoration!.Id.ToString());
        var response = await client.GetAsync(url, cts);

        // Assert
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/json");
    }

    [Fact]
    public async Task GetDecoration_MinimalDecoration_ShouldReturnOk()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        // Create a minimal decoration first
        var decorationName = $"{_faker.Lorem.Word()}-{Guid.NewGuid():N}";
        var createRequest = new DecorationsEndpoints.Add.AddDecorationRequest
        {
            Name = decorationName,
            Category = DecorationCategory.NoCategory.Value
        };
        var createResponse = await client.PostAsJsonAsync(ApiEndpoints.Decorations.Add, createRequest, cts);
        var createdDecoration = await createResponse.Content.ReadFromJsonAsync<DecorationResponse>(cancellationToken: cts);

        // Act
        var url = ApiEndpoints.Decorations.Get.Replace("{id:guid}", createdDecoration!.Id.ToString());
        var response = await client.GetAsync(url, cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var decorationResponse = await response.Content.ReadFromJsonAsync<DecorationResponse>(cancellationToken: cts);
        decorationResponse.ShouldNotBeNull();
        decorationResponse.Name.ShouldBe(decorationName);
        decorationResponse.Category.Value.ShouldBe(DecorationCategory.NoCategory.Value);
    }

    [Theory]
    [InlineData(0)] // NoCategory
    [InlineData(1)] // Stones
    [InlineData(2)] // Sand
    [InlineData(3)] // Figurines
    public async Task GetDecoration_AllCategories_ShouldReturnOk(int categoryValue)
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        // Create a decoration with specific category
        var decorationName = $"{_faker.Lorem.Word()}-{Guid.NewGuid():N}";
        var createRequest = new DecorationsEndpoints.Add.AddDecorationRequest
        {
            Name = decorationName,
            Category = categoryValue,
            Description = _faker.Lorem.Sentence()
        };
        var createResponse = await client.PostAsJsonAsync(ApiEndpoints.Decorations.Add, createRequest, cts);
        var createdDecoration = await createResponse.Content.ReadFromJsonAsync<DecorationResponse>(cancellationToken: cts);

        // Act
        var url = ApiEndpoints.Decorations.Get.Replace("{id:guid}", createdDecoration!.Id.ToString());
        var response = await client.GetAsync(url, cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var decorationResponse = await response.Content.ReadFromJsonAsync<DecorationResponse>(cancellationToken: cts);
        decorationResponse.ShouldNotBeNull();
        decorationResponse.Category.Value.ShouldBe(categoryValue);
    }
}