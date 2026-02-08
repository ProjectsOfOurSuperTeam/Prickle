using System.Net.Http.Json;
using Bogus;
using Prickle.Api.Endpoints;
using Prickle.Application.Decorations;
using Prickle.Domain.Decorations;
using DecorationsEndpoints = Prickle.Api.Endpoints.Decorations;

namespace Prickle.Integration.Tests.Api.DecorationsEndpointTests;

[Collection(nameof(ApiTestsCollectionMarker))]
public sealed class DeleteDecorationEndpointTest : ApiBaseIntegrationTest
{
    private readonly Faker _faker;

    public DeleteDecorationEndpointTest(AppHostFixture appHostFactory) : base(appHostFactory)
    {
        _faker = new Faker();
    }

    [Fact]
    public async Task DeleteDecoration_ExistingId_ShouldReturnNoContent()
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
        var url = ApiEndpoints.Decorations.Delete.Replace("{id:guid}", createdDecoration!.Id.ToString());
        var response = await client.DeleteAsync(url, cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DeleteDecoration_NonExistentId_ShouldReturnBadRequest()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");
        var nonExistentId = Guid.NewGuid();

        // Act
        var url = ApiEndpoints.Decorations.Delete.Replace("{id:guid}", nonExistentId.ToString());
        var response = await client.DeleteAsync(url, cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task DeleteDecoration_InvalidGuidFormat_ShouldReturnNotFound()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        // Act
        var url = ApiEndpoints.Decorations.Delete.Replace("{id:guid}", "invalid-guid");
        var response = await client.DeleteAsync(url, cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteDecoration_AfterDeletion_ShouldNotBeRetrievable()
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

        // Act - Delete the decoration
        var deleteUrl = ApiEndpoints.Decorations.Delete.Replace("{id:guid}", createdDecoration!.Id.ToString());
        var deleteResponse = await client.DeleteAsync(deleteUrl, cts);
        deleteResponse.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        // Assert - Try to get the deleted decoration
        var getUrl = ApiEndpoints.Decorations.Get.Replace("{id:guid}", createdDecoration.Id.ToString());
        var getResponse = await client.GetAsync(getUrl, cts);
        getResponse.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task DeleteDecoration_MultipleDecorations_ShouldDeleteOnlySpecified()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        // Create two decorations
        var firstDecorationName = $"{_faker.Lorem.Word()}-{Guid.NewGuid():N}";
        var secondDecorationName = $"{_faker.Lorem.Word()}-{Guid.NewGuid():N}";

        var firstCreateRequest = new DecorationsEndpoints.Add.AddDecorationRequest
        {
            Name = firstDecorationName,
            Category = DecorationCategory.Stones.Value
        };
        var secondCreateRequest = new DecorationsEndpoints.Add.AddDecorationRequest
        {
            Name = secondDecorationName,
            Category = DecorationCategory.Figurines.Value
        };

        var firstCreateResponse = await client.PostAsJsonAsync(ApiEndpoints.Decorations.Add, firstCreateRequest, cts);
        var secondCreateResponse = await client.PostAsJsonAsync(ApiEndpoints.Decorations.Add, secondCreateRequest, cts);

        var firstDecoration = await firstCreateResponse.Content.ReadFromJsonAsync<DecorationResponse>(cancellationToken: cts);
        var secondDecoration = await secondCreateResponse.Content.ReadFromJsonAsync<DecorationResponse>(cancellationToken: cts);

        // Act - Delete only the first decoration
        var deleteUrl = ApiEndpoints.Decorations.Delete.Replace("{id:guid}", firstDecoration!.Id.ToString());
        var deleteResponse = await client.DeleteAsync(deleteUrl, cts);

        // Assert
        deleteResponse.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        // Verify first decoration is gone
        var firstGetUrl = ApiEndpoints.Decorations.Get.Replace("{id:guid}", firstDecoration.Id.ToString());
        var firstGetResponse = await client.GetAsync(firstGetUrl, cts);
        firstGetResponse.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        // Verify second decoration still exists
        var secondGetUrl = ApiEndpoints.Decorations.Get.Replace("{id:guid}", secondDecoration!.Id.ToString());
        var secondGetResponse = await client.GetAsync(secondGetUrl, cts);
        secondGetResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Theory]
    [InlineData(0)] // NoCategory
    [InlineData(1)] // Stones
    [InlineData(2)] // Sand
    [InlineData(3)] // Figurines
    public async Task DeleteDecoration_AllCategories_ShouldDeleteSuccessfully(int categoryValue)
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
        var url = ApiEndpoints.Decorations.Delete.Replace("{id:guid}", createdDecoration!.Id.ToString());
        var response = await client.DeleteAsync(url, cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DeleteDecoration_WithAllOptionalFields_ShouldDeleteSuccessfully()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        // Create a decoration with all optional fields
        var decorationName = $"{_faker.Lorem.Word()}-{Guid.NewGuid():N}";
        var createRequest = new DecorationsEndpoints.Add.AddDecorationRequest
        {
            Name = decorationName,
            Description = _faker.Lorem.Sentence(10),
            Category = DecorationCategory.Figurines.Value,
            ImageUrl = _faker.Image.PicsumUrl(),
            ImageIsometricUrl = _faker.Image.PicsumUrl()
        };
        var createResponse = await client.PostAsJsonAsync(ApiEndpoints.Decorations.Add, createRequest, cts);
        var createdDecoration = await createResponse.Content.ReadFromJsonAsync<DecorationResponse>(cancellationToken: cts);

        // Act
        var url = ApiEndpoints.Decorations.Delete.Replace("{id:guid}", createdDecoration!.Id.ToString());
        var response = await client.DeleteAsync(url, cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DeleteDecoration_MinimalDecoration_ShouldDeleteSuccessfully()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        // Create a minimal decoration
        var decorationName = $"{_faker.Lorem.Word()}-{Guid.NewGuid():N}";
        var createRequest = new DecorationsEndpoints.Add.AddDecorationRequest
        {
            Name = decorationName,
            Category = DecorationCategory.NoCategory.Value
        };
        var createResponse = await client.PostAsJsonAsync(ApiEndpoints.Decorations.Add, createRequest, cts);
        var createdDecoration = await createResponse.Content.ReadFromJsonAsync<DecorationResponse>(cancellationToken: cts);

        // Act
        var url = ApiEndpoints.Decorations.Delete.Replace("{id:guid}", createdDecoration!.Id.ToString());
        var response = await client.DeleteAsync(url, cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DeleteDecoration_EmptyGuid_ShouldReturnBadRequest()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        // Act
        var url = ApiEndpoints.Decorations.Delete.Replace("{id:guid}", Guid.Empty.ToString());
        var response = await client.DeleteAsync(url, cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task DeleteDecoration_DuplicateDelete_ShouldReturnBadRequest()
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

        var url = ApiEndpoints.Decorations.Delete.Replace("{id:guid}", createdDecoration!.Id.ToString());

        // Act - Delete first time
        var firstDeleteResponse = await client.DeleteAsync(url, cts);
        firstDeleteResponse.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        // Act - Try to delete again
        var secondDeleteResponse = await client.DeleteAsync(url, cts);

        // Assert
        secondDeleteResponse.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }
}