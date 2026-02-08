using System.Net.Http.Json;
using Bogus;
using Prickle.Api.Endpoints;
using Prickle.Application.Decorations;
using Prickle.Domain.Decorations;
using DecorationsEndpoints = Prickle.Api.Endpoints.Decorations;

namespace Prickle.Integration.Tests.Api.DecorationsEndpointTests;

[Collection(nameof(ApiTestsCollectionMarker))]
public sealed class UpdateDecorationEndpointTest : ApiBaseIntegrationTest
{
    private readonly Faker _faker;
    private readonly ITestOutputHelper _outputHelper;

    public UpdateDecorationEndpointTest(AppHostFixture appHostFactory, ITestOutputHelper outputHelper) : base(appHostFactory)
    {
        _faker = new Faker();
        _outputHelper = outputHelper;
    }

    [Fact]
    public async Task UpdateDecoration_ValidRequest_ShouldReturnOkWithUpdatedDecoration()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        // Create a decoration first
        var originalName = $"{_faker.Lorem.Word()}-{Guid.NewGuid():N}";
        var createRequest = new DecorationsEndpoints.Add.AddDecorationRequest
        {
            Name = originalName,
            Description = _faker.Lorem.Sentence(),
            Category = DecorationCategory.Stones.Value,
            ImageUrl = "https://example.com/original.jpg",
            ImageIsometricUrl = "https://example.com/original-isometric.jpg"
        };
        var createResponse = await client.PostAsJsonAsync(ApiEndpoints.Decorations.Add, createRequest, cts);
        var createdDecoration = await createResponse.Content.ReadFromJsonAsync<DecorationResponse>(cancellationToken: cts);

        // Act
        var newName = $"{_faker.Lorem.Word()}-{Guid.NewGuid():N}";
        var newDescription = _faker.Lorem.Sentence(8);
        var updateRequest = new DecorationsEndpoints.Update.UpdateDecorationRequest
        {
            Name = newName,
            Description = newDescription,
            Category = DecorationCategory.Sand.Value,
            ImageUrl = "https://example.com/updated.jpg",
            ImageIsometricUrl = "https://example.com/updated-isometric.jpg"
        };
        var url = ApiEndpoints.Decorations.Update.Replace("{id:guid}", createdDecoration!.Id.ToString());
        var response = await client.PatchAsJsonAsync(url, updateRequest, cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var updatedDecoration = await response.Content.ReadFromJsonAsync<DecorationResponse>(cancellationToken: cts);
        updatedDecoration.ShouldNotBeNull();
        updatedDecoration.Id.ShouldBe(createdDecoration.Id);
        updatedDecoration.Name.ShouldBe(newName);
        updatedDecoration.Description.ShouldBe(newDescription);
        updatedDecoration.Category.Value.ShouldBe(DecorationCategory.Sand.Value);
        updatedDecoration.ImageUrl.ShouldBe(updateRequest.ImageUrl);
        updatedDecoration.ImageIsometricUrl.ShouldBe(updateRequest.ImageIsometricUrl);
    }

    [Fact]
    public async Task UpdateDecoration_NonExistentId_ShouldReturnBadRequest()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");
        var nonExistentId = Guid.NewGuid();

        var updateRequest = new DecorationsEndpoints.Update.UpdateDecorationRequest
        {
            Name = _faker.Lorem.Word(),
            Category = DecorationCategory.Stones.Value
        };

        // Act
        var url = ApiEndpoints.Decorations.Update.Replace("{id:guid}", nonExistentId.ToString());
        var response = await client.PatchAsJsonAsync(url, updateRequest, cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateDecoration_EmptyName_ShouldReturnBadRequest()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        // Create a decoration first
        var originalName = $"{_faker.Lorem.Word()}-{Guid.NewGuid():N}";
        var createRequest = new DecorationsEndpoints.Add.AddDecorationRequest
        {
            Name = originalName,
            Category = DecorationCategory.Stones.Value
        };
        var createResponse = await client.PostAsJsonAsync(ApiEndpoints.Decorations.Add, createRequest, cts);
        var createdDecoration = await createResponse.Content.ReadFromJsonAsync<DecorationResponse>(cancellationToken: cts);

        // Act
        var updateRequest = new DecorationsEndpoints.Update.UpdateDecorationRequest
        {
            Name = string.Empty,
            Category = DecorationCategory.Sand.Value
        };
        var url = ApiEndpoints.Decorations.Update.Replace("{id:guid}", createdDecoration!.Id.ToString());
        var response = await client.PatchAsJsonAsync(url, updateRequest, cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateDecoration_NameTooLong_ShouldReturnBadRequest()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        // Create a decoration first
        var originalName = $"{_faker.Lorem.Word()}-{Guid.NewGuid():N}";
        var createRequest = new DecorationsEndpoints.Add.AddDecorationRequest
        {
            Name = originalName,
            Category = DecorationCategory.Figurines.Value
        };
        var createResponse = await client.PostAsJsonAsync(ApiEndpoints.Decorations.Add, createRequest, cts);
        var createdDecoration = await createResponse.Content.ReadFromJsonAsync<DecorationResponse>(cancellationToken: cts);

        // Act
        var longName = _faker.Random.String2(256); // Max is 255
        var updateRequest = new DecorationsEndpoints.Update.UpdateDecorationRequest
        {
            Name = longName,
            Category = DecorationCategory.Sand.Value
        };
        var url = ApiEndpoints.Decorations.Update.Replace("{id:guid}", createdDecoration!.Id.ToString());
        var response = await client.PatchAsJsonAsync(url, updateRequest, cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateDecoration_DuplicateName_ShouldReturnBadRequest()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        // Create two decorations
        var firstName = $"{_faker.Lorem.Word()}-{Guid.NewGuid():N}";
        var secondName = $"{_faker.Lorem.Word()}-{Guid.NewGuid():N}";

        var firstCreateRequest = new DecorationsEndpoints.Add.AddDecorationRequest
        {
            Name = firstName,
            Category = DecorationCategory.Stones.Value
        };
        var secondCreateRequest = new DecorationsEndpoints.Add.AddDecorationRequest
        {
            Name = secondName,
            Category = DecorationCategory.Sand.Value
        };

        var firstCreateResponse = await client.PostAsJsonAsync(ApiEndpoints.Decorations.Add, firstCreateRequest, cts);
        var secondCreateResponse = await client.PostAsJsonAsync(ApiEndpoints.Decorations.Add, secondCreateRequest, cts);

        var firstDecoration = await firstCreateResponse.Content.ReadFromJsonAsync<DecorationResponse>(cancellationToken: cts);
        var secondDecoration = await secondCreateResponse.Content.ReadFromJsonAsync<DecorationResponse>(cancellationToken: cts);

        // Act - Try to update second decoration with first decoration's name
        var updateRequest = new DecorationsEndpoints.Update.UpdateDecorationRequest
        {
            Name = firstName, // This should conflict
            Category = DecorationCategory.Figurines.Value
        };
        var url = ApiEndpoints.Decorations.Update.Replace("{id:guid}", secondDecoration!.Id.ToString());
        var response = await client.PatchAsJsonAsync(url, updateRequest, cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateDecoration_DuplicateNameDifferentCasing_ShouldReturnBadRequest()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        var firstName = $"{_faker.Lorem.Word()}-{Guid.NewGuid():N}".ToLowerInvariant();
        var secondName = $"{_faker.Lorem.Word()}-{Guid.NewGuid():N}";

        // Create two decorations
        var firstCreateRequest = new DecorationsEndpoints.Add.AddDecorationRequest
        {
            Name = firstName,
            Category = DecorationCategory.Stones.Value
        };
        var secondCreateRequest = new DecorationsEndpoints.Add.AddDecorationRequest
        {
            Name = secondName,
            Category = DecorationCategory.Sand.Value
        };

        var firstCreateResponse = await client.PostAsJsonAsync(ApiEndpoints.Decorations.Add, firstCreateRequest, cts);
        var secondCreateResponse = await client.PostAsJsonAsync(ApiEndpoints.Decorations.Add, secondCreateRequest, cts);

        var secondDecoration = await secondCreateResponse.Content.ReadFromJsonAsync<DecorationResponse>(cancellationToken: cts);

        // Act - Try to update with uppercase version of existing name
        var updateRequest = new DecorationsEndpoints.Update.UpdateDecorationRequest
        {
            Name = firstName.ToUpperInvariant(),
            Category = DecorationCategory.Figurines.Value
        };
        var url = ApiEndpoints.Decorations.Update.Replace("{id:guid}", secondDecoration!.Id.ToString());
        var response = await client.PatchAsJsonAsync(url, updateRequest, cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateDecoration_SameName_ShouldReturnOk()
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
            Category = DecorationCategory.Stones.Value,
            Description = "Original description"
        };
        var createResponse = await client.PostAsJsonAsync(ApiEndpoints.Decorations.Add, createRequest, cts);
        var createdDecoration = await createResponse.Content.ReadFromJsonAsync<DecorationResponse>(cancellationToken: cts);

        // Act - Update with same name but different category
        var updateRequest = new DecorationsEndpoints.Update.UpdateDecorationRequest
        {
            Name = decorationName, // Same name
            Category = DecorationCategory.Sand.Value, // Different category
            Description = "Updated description"
        };
        var url = ApiEndpoints.Decorations.Update.Replace("{id:guid}", createdDecoration!.Id.ToString());
        var response = await client.PatchAsJsonAsync(url, updateRequest, cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var updatedDecoration = await response.Content.ReadFromJsonAsync<DecorationResponse>(cancellationToken: cts);
        updatedDecoration.ShouldNotBeNull();
        updatedDecoration.Name.ShouldBe(decorationName);
        updatedDecoration.Category.Value.ShouldBe(DecorationCategory.Sand.Value);
        updatedDecoration.Description.ShouldBe("Updated description");
    }

    [Fact]
    public async Task UpdateDecoration_NameWithWhitespace_ShouldTrimAndReturnOk()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        // Create a decoration first
        var originalName = $"{_faker.Lorem.Word()}-{Guid.NewGuid():N}";
        var createRequest = new DecorationsEndpoints.Add.AddDecorationRequest
        {
            Name = originalName,
            Category = DecorationCategory.Figurines.Value
        };
        var createResponse = await client.PostAsJsonAsync(ApiEndpoints.Decorations.Add, createRequest, cts);
        var createdDecoration = await createResponse.Content.ReadFromJsonAsync<DecorationResponse>(cancellationToken: cts);

        // Act
        var newName = $"{_faker.Lorem.Word()}-{Guid.NewGuid():N}";
        var updateRequest = new DecorationsEndpoints.Update.UpdateDecorationRequest
        {
            Name = $"  {newName}  ", // With whitespace
            Category = DecorationCategory.Sand.Value
        };
        var url = ApiEndpoints.Decorations.Update.Replace("{id:guid}", createdDecoration!.Id.ToString());
        var response = await client.PatchAsJsonAsync(url, updateRequest, cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var updatedDecoration = await response.Content.ReadFromJsonAsync<DecorationResponse>(cancellationToken: cts);
        updatedDecoration.ShouldNotBeNull();
        updatedDecoration.Name.ShouldBe(newName); // Should be trimmed
    }

    [Fact]
    public async Task UpdateDecoration_InvalidCategory_ShouldReturnBadRequest()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        // Create a decoration first
        var originalName = $"{_faker.Lorem.Word()}-{Guid.NewGuid():N}";
        var createRequest = new DecorationsEndpoints.Add.AddDecorationRequest
        {
            Name = originalName,
            Category = DecorationCategory.Stones.Value
        };
        var createResponse = await client.PostAsJsonAsync(ApiEndpoints.Decorations.Add, createRequest, cts);
        var createdDecoration = await createResponse.Content.ReadFromJsonAsync<DecorationResponse>(cancellationToken: cts);

        // Act
        var updateRequest = new DecorationsEndpoints.Update.UpdateDecorationRequest
        {
            Name = _faker.Lorem.Word(),
            Category = 999 // Invalid category
        };
        var url = ApiEndpoints.Decorations.Update.Replace("{id:guid}", createdDecoration!.Id.ToString());
        var response = await client.PatchAsJsonAsync(url, updateRequest, cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Theory]
    [InlineData(0)] // NoCategory
    [InlineData(1)] // Stones
    [InlineData(2)] // Sand
    [InlineData(3)] // Figurines
    public async Task UpdateDecoration_AllValidCategories_ShouldReturnOk(int categoryValue)
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        // Create a decoration first
        var originalName = $"{_faker.Lorem.Word()}-{Guid.NewGuid():N}";
        var createRequest = new DecorationsEndpoints.Add.AddDecorationRequest
        {
            Name = originalName,
            Category = DecorationCategory.NoCategory.Value
        };
        var createResponse = await client.PostAsJsonAsync(ApiEndpoints.Decorations.Add, createRequest, cts);
        var createdDecoration = await createResponse.Content.ReadFromJsonAsync<DecorationResponse>(cancellationToken: cts);

        // Act
        var newName = $"{_faker.Lorem.Word()}-{Guid.NewGuid():N}";
        var updateRequest = new DecorationsEndpoints.Update.UpdateDecorationRequest
        {
            Name = newName,
            Category = categoryValue
        };
        var url = ApiEndpoints.Decorations.Update.Replace("{id:guid}", createdDecoration!.Id.ToString());
        var response = await client.PatchAsJsonAsync(url, updateRequest, cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var updatedDecoration = await response.Content.ReadFromJsonAsync<DecorationResponse>(cancellationToken: cts);
        updatedDecoration.ShouldNotBeNull();
        updatedDecoration.Category.Value.ShouldBe(categoryValue);
    }

    [Fact]
    public async Task UpdateDecoration_DescriptionTooLong_ShouldReturnBadRequest()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        // Create a decoration first
        var originalName = $"{_faker.Lorem.Word()}-{Guid.NewGuid():N}";
        var createRequest = new DecorationsEndpoints.Add.AddDecorationRequest
        {
            Name = originalName,
            Category = DecorationCategory.Stones.Value
        };
        var createResponse = await client.PostAsJsonAsync(ApiEndpoints.Decorations.Add, createRequest, cts);
        var createdDecoration = await createResponse.Content.ReadFromJsonAsync<DecorationResponse>(cancellationToken: cts);

        // Act
        var longDescription = _faker.Random.String2(501); // Max is 500
        var updateRequest = new DecorationsEndpoints.Update.UpdateDecorationRequest
        {
            Name = _faker.Lorem.Word(),
            Category = DecorationCategory.Sand.Value,
            Description = longDescription
        };
        var url = ApiEndpoints.Decorations.Update.Replace("{id:guid}", createdDecoration!.Id.ToString());
        var response = await client.PatchAsJsonAsync(url, updateRequest, cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateDecoration_WithAllOptionalFields_ShouldReturnOk()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        // Create a decoration first
        var originalName = $"{_faker.Lorem.Word()}-{Guid.NewGuid():N}";
        var createRequest = new DecorationsEndpoints.Add.AddDecorationRequest
        {
            Name = originalName,
            Category = DecorationCategory.NoCategory.Value
        };
        var createResponse = await client.PostAsJsonAsync(ApiEndpoints.Decorations.Add, createRequest, cts);
        var createdDecoration = await createResponse.Content.ReadFromJsonAsync<DecorationResponse>(cancellationToken: cts);

        // Act
        var newName = $"{_faker.Lorem.Word()}-{Guid.NewGuid():N}";
        var updateRequest = new DecorationsEndpoints.Update.UpdateDecorationRequest
        {
            Name = newName,
            Description = _faker.Lorem.Sentence(10),
            Category = DecorationCategory.Figurines.Value,
            ImageUrl = _faker.Image.PicsumUrl(),
            ImageIsometricUrl = _faker.Image.PicsumUrl()
        };
        var url = ApiEndpoints.Decorations.Update.Replace("{id:guid}", createdDecoration!.Id.ToString());
        var response = await client.PatchAsJsonAsync(url, updateRequest, cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var updatedDecoration = await response.Content.ReadFromJsonAsync<DecorationResponse>(cancellationToken: cts);
        updatedDecoration.ShouldNotBeNull();
        updatedDecoration.Name.ShouldBe(newName);
        updatedDecoration.Description.ShouldBe(updateRequest.Description);
        updatedDecoration.Category.Value.ShouldBe(DecorationCategory.Figurines.Value);
        updatedDecoration.ImageUrl.ShouldBe(updateRequest.ImageUrl);
        updatedDecoration.ImageIsometricUrl.ShouldBe(updateRequest.ImageIsometricUrl);
    }

    [Fact]
    public async Task UpdateDecoration_InvalidGuidFormat_ShouldReturnNotFound()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        var updateRequest = new DecorationsEndpoints.Update.UpdateDecorationRequest
        {
            Name = _faker.Lorem.Word(),
            Category = DecorationCategory.Stones.Value
        };

        // Act
        var url = ApiEndpoints.Decorations.Update.Replace("{id:guid}", "invalid-guid");
        var response = await client.PatchAsJsonAsync(url, updateRequest, cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
}