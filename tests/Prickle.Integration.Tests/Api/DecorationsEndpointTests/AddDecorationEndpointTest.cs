using System.Net.Http.Json;
using Bogus;
using Prickle.Api.Endpoints;
using Prickle.Application.Decorations;
using Prickle.Domain.Decorations;
using DecorationsEndpoints = Prickle.Api.Endpoints.Decorations;

namespace Prickle.Integration.Tests.Api.DecorationsEndpointTests;

[Collection(nameof(ApiTestsCollectionMarker))]
public sealed class AddDecorationEndpointTest : ApiBaseIntegrationTest
{
    private readonly Faker _faker;

    public AddDecorationEndpointTest(AppHostFixture appHostFactory) : base(appHostFactory)
    {
        _faker = new Faker();
    }

    [Fact]
    public async Task AddDecoration_ValidRequest_ShouldReturnCreatedWithDecoration()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        var newDecoration = new DecorationsEndpoints.Add.AddDecorationRequest
        {
            Name = $"{_faker.Lorem.Word()}-{Guid.NewGuid():N}",
            Description = _faker.Lorem.Sentence(),
            Category = DecorationCategory.Stones.Value,
            ImageUrl = "https://example.com/image.jpg",
            ImageIsometricUrl = "https://example.com/isometric.jpg"
        };

        // Act
        var response = await client.PostAsJsonAsync(ApiEndpoints.Decorations.Add, newDecoration, cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        var decorationResponse = await response.Content.ReadFromJsonAsync<DecorationResponse>(cancellationToken: cts);
        decorationResponse.ShouldNotBeNull();
        decorationResponse.Id.ShouldNotBe(Guid.Empty);
        decorationResponse.Name.ShouldBe(newDecoration.Name);
        decorationResponse.Description.ShouldBe(newDecoration.Description);
        decorationResponse.Category.Value.ShouldBe(DecorationCategory.Stones.Value);
        decorationResponse.ImageUrl.ShouldBe(newDecoration.ImageUrl);
        decorationResponse.ImageIsometricUrl.ShouldBe(newDecoration.ImageIsometricUrl);

        response.Headers.Location.ShouldNotBeNull();
    }

    [Fact]
    public async Task AddDecoration_EmptyName_ShouldReturnBadRequest()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        var newDecoration = new DecorationsEndpoints.Add.AddDecorationRequest
        {
            Name = string.Empty,
            Category = DecorationCategory.Sand.Value
        };

        // Act
        var response = await client.PostAsJsonAsync(ApiEndpoints.Decorations.Add, newDecoration, cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task AddDecoration_NameTooLong_ShouldReturnBadRequest()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        var longName = _faker.Random.String2(256); // Max is 255
        var newDecoration = new DecorationsEndpoints.Add.AddDecorationRequest
        {
            Name = longName,
            Category = DecorationCategory.Figurines.Value
        };

        // Act
        var response = await client.PostAsJsonAsync(ApiEndpoints.Decorations.Add, newDecoration, cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task AddDecoration_DuplicateName_ShouldReturnBadRequest()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        var decorationName = $"{_faker.Lorem.Word()}-{Guid.NewGuid():N}";
        var newDecoration = new DecorationsEndpoints.Add.AddDecorationRequest
        {
            Name = decorationName,
            Category = DecorationCategory.Stones.Value
        };

        // Act - Add the decoration first time
        var firstResponse = await client.PostAsJsonAsync(ApiEndpoints.Decorations.Add, newDecoration, cts);
        firstResponse.StatusCode.ShouldBe(HttpStatusCode.Created);

        // Act - Try to add the same decoration again
        var secondResponse = await client.PostAsJsonAsync(ApiEndpoints.Decorations.Add, newDecoration, cts);

        // Assert
        secondResponse.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task AddDecoration_DuplicateNameDifferentCasing_ShouldReturnBadRequest()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        var baseDecorationName = $"{_faker.Lorem.Word()}-{Guid.NewGuid():N}";
        var decorationNameLower = baseDecorationName.ToLowerInvariant();

        var firstRequest = new DecorationsEndpoints.Add.AddDecorationRequest
        {
            Name = decorationNameLower,
            Category = DecorationCategory.Sand.Value
        };
        var secondRequest = new DecorationsEndpoints.Add.AddDecorationRequest
        {
            Name = decorationNameLower.ToUpperInvariant(),
            Category = DecorationCategory.Sand.Value
        };

        // Act - Add the decoration with lowercase
        var firstResponse = await client.PostAsJsonAsync(ApiEndpoints.Decorations.Add, firstRequest, cts);
        firstResponse.StatusCode.ShouldBe(HttpStatusCode.Created);

        // Act - Try to add with uppercase
        var secondResponse = await client.PostAsJsonAsync(ApiEndpoints.Decorations.Add, secondRequest, cts);

        // Assert
        secondResponse.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task AddDecoration_NameWithWhitespace_ShouldTrimAndReturnCreated()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        var decorationName = $"{_faker.Lorem.Word()}-{Guid.NewGuid():N}";
        var newDecoration = new DecorationsEndpoints.Add.AddDecorationRequest
        {
            Name = $"  {decorationName}  ",
            Category = DecorationCategory.Figurines.Value
        };

        // Act
        var response = await client.PostAsJsonAsync(ApiEndpoints.Decorations.Add, newDecoration, cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        var decorationResponse = await response.Content.ReadFromJsonAsync<DecorationResponse>(cancellationToken: cts);
        decorationResponse.ShouldNotBeNull();
        decorationResponse.Name.ShouldBe(decorationName);
    }

    [Fact]
    public async Task AddDecoration_InvalidCategory_ShouldReturnBadRequest()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        var newDecoration = new DecorationsEndpoints.Add.AddDecorationRequest
        {
            Name = _faker.Lorem.Word(),
            Category = 999 // Invalid category
        };

        // Act
        var response = await client.PostAsJsonAsync(ApiEndpoints.Decorations.Add, newDecoration, cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task AddDecoration_DescriptionTooLong_ShouldReturnBadRequest()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        var longDescription = _faker.Random.String2(501); // Max is 500
        var newDecoration = new DecorationsEndpoints.Add.AddDecorationRequest
        {
            Name = _faker.Lorem.Word(),
            Description = longDescription,
            Category = DecorationCategory.Stones.Value
        };

        // Act
        var response = await client.PostAsJsonAsync(ApiEndpoints.Decorations.Add, newDecoration, cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task AddDecoration_ImageUrlTooLong_ShouldReturnBadRequest()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        var longUrl = $"https://example.com/{_faker.Random.String2(2049)}"; // Max is 2048
        var newDecoration = new DecorationsEndpoints.Add.AddDecorationRequest
        {
            Name = _faker.Lorem.Word(),
            Category = DecorationCategory.Sand.Value,
            ImageUrl = longUrl
        };

        // Act
        var response = await client.PostAsJsonAsync(ApiEndpoints.Decorations.Add, newDecoration, cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task AddDecoration_ImageIsometricUrlTooLong_ShouldReturnBadRequest()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        var longUrl = $"https://example.com/{_faker.Random.String2(2049)}"; // Max is 2048
        var newDecoration = new DecorationsEndpoints.Add.AddDecorationRequest
        {
            Name = _faker.Lorem.Word(),
            Category = DecorationCategory.Figurines.Value,
            ImageIsometricUrl = longUrl
        };

        // Act
        var response = await client.PostAsJsonAsync(ApiEndpoints.Decorations.Add, newDecoration, cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task AddDecoration_MinimalRequest_ShouldReturnCreated()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        var newDecoration = new DecorationsEndpoints.Add.AddDecorationRequest
        {
            Name = $"{_faker.Lorem.Word()}-{Guid.NewGuid():N}",
            Category = DecorationCategory.Stones.Value
        };

        // Act
        var response = await client.PostAsJsonAsync(ApiEndpoints.Decorations.Add, newDecoration, cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        var decorationResponse = await response.Content.ReadFromJsonAsync<DecorationResponse>(cancellationToken: cts);
        decorationResponse.ShouldNotBeNull();
        decorationResponse.Name.ShouldBe(newDecoration.Name);
        decorationResponse.Category.Value.ShouldBe(DecorationCategory.Stones.Value);
    }

    [Theory]
    [InlineData("Premium Stone")]
    [InlineData("Garden Rock 123")]
    [InlineData("Rock-A1")]
    [InlineData("Natural Figurine")]
    [InlineData("Sand Mix v2")]
    public async Task AddDecoration_VariousValidNames_ShouldReturnCreated(string decorationNamePrefix)
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        // Make the name unique by adding a GUID suffix to avoid conflicts with other tests
        var decorationName = $"{decorationNamePrefix}-{Guid.NewGuid():N}";
        var newDecoration = new DecorationsEndpoints.Add.AddDecorationRequest
        {
            Name = decorationName,
            Category = DecorationCategory.Stones.Value
        };

        // Act
        var response = await client.PostAsJsonAsync(ApiEndpoints.Decorations.Add, newDecoration, cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        var decorationResponse = await response.Content.ReadFromJsonAsync<DecorationResponse>(cancellationToken: cts);
        decorationResponse.ShouldNotBeNull();
        decorationResponse.Name.ShouldBe(decorationName);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public async Task AddDecoration_AllValidCategories_ShouldReturnCreated(int categoryValue)
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        var newDecoration = new DecorationsEndpoints.Add.AddDecorationRequest
        {
            Name = $"{_faker.Lorem.Word()}-{Guid.NewGuid():N}",
            Category = categoryValue,
            Description = _faker.Lorem.Sentence()
        };

        // Act
        var response = await client.PostAsJsonAsync(ApiEndpoints.Decorations.Add, newDecoration, cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        var decorationResponse = await response.Content.ReadFromJsonAsync<DecorationResponse>(cancellationToken: cts);
        decorationResponse.ShouldNotBeNull();
        decorationResponse.Category.Value.ShouldBe(categoryValue);
    }

    [Fact]
    public async Task AddDecoration_WithAllOptionalFields_ShouldReturnCreated()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        var newDecoration = new DecorationsEndpoints.Add.AddDecorationRequest
        {
            Name = $"{_faker.Lorem.Word()}-{Guid.NewGuid():N}",
            Description = _faker.Lorem.Sentence(10),
            Category = DecorationCategory.Figurines.Value,
            ImageUrl = _faker.Image.PicsumUrl(),
            ImageIsometricUrl = _faker.Image.PicsumUrl()
        };

        // Act
        var response = await client.PostAsJsonAsync(ApiEndpoints.Decorations.Add, newDecoration, cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        var decorationResponse = await response.Content.ReadFromJsonAsync<DecorationResponse>(cancellationToken: cts);
        decorationResponse.ShouldNotBeNull();
        decorationResponse.Description.ShouldBe(newDecoration.Description);
        decorationResponse.ImageUrl.ShouldBe(newDecoration.ImageUrl);
        decorationResponse.ImageIsometricUrl.ShouldBe(newDecoration.ImageIsometricUrl);
    }

    [Fact]
    public async Task AddDecoration_CreatedResponseIncludesLocation_ShouldHaveCorrectHeader()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        var newDecoration = new DecorationsEndpoints.Add.AddDecorationRequest
        {
            Name = $"{_faker.Lorem.Word()}-{Guid.NewGuid():N}",
            Category = DecorationCategory.Sand.Value
        };

        // Act
        var response = await client.PostAsJsonAsync(ApiEndpoints.Decorations.Add, newDecoration, cts);
        var decorationResponse = await response.Content.ReadFromJsonAsync<DecorationResponse>(cancellationToken: cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        response.Headers.Location.ShouldNotBeNull();
        response.Headers.Location!.OriginalString.ShouldContain(decorationResponse!.Id.ToString());
    }
}