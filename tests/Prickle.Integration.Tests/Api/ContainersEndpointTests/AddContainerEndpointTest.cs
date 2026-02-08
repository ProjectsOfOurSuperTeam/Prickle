using System.Net.Http.Json;
using Bogus;
using Prickle.Api.Endpoints;
using Prickle.Application.Containers;
using ContainersEndpoints = Prickle.Api.Endpoints.Containers;

namespace Prickle.Integration.Tests.Api.ContainersEndpointTests;

[Collection(nameof(ApiTestsCollectionMarker))]
public sealed class AddContainerEndpointTest : ApiBaseIntegrationTest
{
    private readonly Faker _faker;

    public AddContainerEndpointTest(AppHostFixture appHostFactory) : base(appHostFactory)
    {
        _faker = new Faker();
    }

    [Fact]
    public async Task AddContainer_ValidRequest_ShouldReturnCreatedWithContainer()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        var newContainer = new ContainersEndpoints.Add.AddContainerRequest
        {
            Name = $"{_faker.Lorem.Word()}-{Guid.NewGuid():N}",
            Description = _faker.Lorem.Sentence(),
            Volume = _faker.Random.Float(1.0f, 100.0f),
            IsClosed = _faker.Random.Bool(),
            ImageUrl = "https://example.com/image.jpg",
            ImageIsometricUrl = "https://example.com/isometric.jpg"
        };

        // Act
        var response = await client.PostAsJsonAsync(ApiEndpoints.Containers.Add, newContainer, cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        var containerResponse = await response.Content.ReadFromJsonAsync<ContainerResponse>(cancellationToken: cts);
        containerResponse.ShouldNotBeNull();
        containerResponse.Id.ShouldNotBe(Guid.Empty);
        containerResponse.Name.ShouldBe(newContainer.Name);
        containerResponse.Description.ShouldBe(newContainer.Description);
        containerResponse.Volume.ShouldBe(newContainer.Volume);
        containerResponse.IsClosed.ShouldBe(newContainer.IsClosed);
        containerResponse.ImageUrl.ShouldBe(newContainer.ImageUrl);
        containerResponse.ImageIsometricUrl.ShouldBe(newContainer.ImageIsometricUrl);

        var locationHeader = response.Headers.Location;
        locationHeader.ShouldNotBeNull();
        locationHeader.ToString().ShouldContain(containerResponse.Id.ToString());
    }

    [Fact]
    public async Task AddContainer_EmptyName_ShouldReturnBadRequest()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        var newContainer = new ContainersEndpoints.Add.AddContainerRequest
        {
            Name = string.Empty,
            Description = _faker.Lorem.Sentence(),
            Volume = _faker.Random.Float(1.0f, 100.0f),
            IsClosed = _faker.Random.Bool(),
            ImageUrl = "https://example.com/image.jpg",
            ImageIsometricUrl = "https://example.com/isometric.jpg"
        };

        // Act
        var response = await client.PostAsJsonAsync(ApiEndpoints.Containers.Add, newContainer, cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task AddContainer_NullName_ShouldReturnBadRequest()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        var newContainer = new ContainersEndpoints.Add.AddContainerRequest
        {
            Name = null!,
            Description = _faker.Lorem.Sentence(),
            Volume = _faker.Random.Float(1.0f, 100.0f),
            IsClosed = _faker.Random.Bool(),
            ImageUrl = "https://example.com/image.jpg",
            ImageIsometricUrl = "https://example.com/isometric.jpg"
        };

        // Act
        var response = await client.PostAsJsonAsync(ApiEndpoints.Containers.Add, newContainer, cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task AddContainer_InvalidVolume_ShouldReturnBadRequest()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        var newContainer = new ContainersEndpoints.Add.AddContainerRequest
        {
            Name = $"{_faker.Lorem.Word()}-{Guid.NewGuid():N}",
            Description = _faker.Lorem.Sentence(),
            Volume = 0,
            IsClosed = _faker.Random.Bool(),
            ImageUrl = "https://example.com/image.jpg",
            ImageIsometricUrl = "https://example.com/isometric.jpg"
        };

        // Act
        var response = await client.PostAsJsonAsync(ApiEndpoints.Containers.Add, newContainer, cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task AddContainer_NegativeVolume_ShouldReturnBadRequest()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        var newContainer = new ContainersEndpoints.Add.AddContainerRequest
        {
            Name = $"{_faker.Lorem.Word()}-{Guid.NewGuid():N}",
            Description = _faker.Lorem.Sentence(),
            Volume = -10.0f,
            IsClosed = _faker.Random.Bool(),
            ImageUrl = "https://example.com/image.jpg",
            ImageIsometricUrl = "https://example.com/isometric.jpg"
        };

        // Act
        var response = await client.PostAsJsonAsync(ApiEndpoints.Containers.Add, newContainer, cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task AddContainer_DuplicateName_ShouldReturnBadRequest()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        var containerName = $"{_faker.Lorem.Word()}-{Guid.NewGuid():N}";
        var firstContainer = new ContainersEndpoints.Add.AddContainerRequest
        {
            Name = containerName,
            Description = _faker.Lorem.Sentence(),
            Volume = _faker.Random.Float(1.0f, 100.0f),
            IsClosed = _faker.Random.Bool(),
            ImageUrl = "https://example.com/image.jpg",
            ImageIsometricUrl = "https://example.com/isometric.jpg"
        };

        // Create first container
        var firstResponse = await client.PostAsJsonAsync(ApiEndpoints.Containers.Add, firstContainer, cts);
        firstResponse.StatusCode.ShouldBe(HttpStatusCode.Created);

        var duplicateContainer = new ContainersEndpoints.Add.AddContainerRequest
        {
            Name = containerName,
            Description = _faker.Lorem.Sentence(),
            Volume = _faker.Random.Float(1.0f, 100.0f),
            IsClosed = _faker.Random.Bool(),
            ImageUrl = "https://example.com/image2.jpg",
            ImageIsometricUrl = "https://example.com/isometric2.jpg"
        };

        // Act
        var response = await client.PostAsJsonAsync(ApiEndpoints.Containers.Add, duplicateContainer, cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task AddContainer_MinimalValidRequest_ShouldReturnCreated()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        var newContainer = new ContainersEndpoints.Add.AddContainerRequest
        {
            Name = $"{_faker.Lorem.Word()}-{Guid.NewGuid():N}",
            Description = null,
            Volume = 1.0f,
            IsClosed = false,
            ImageUrl = null,
            ImageIsometricUrl = null
        };

        // Act
        var response = await client.PostAsJsonAsync(ApiEndpoints.Containers.Add, newContainer, cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        var containerResponse = await response.Content.ReadFromJsonAsync<ContainerResponse>(cancellationToken: cts);
        containerResponse.ShouldNotBeNull();
        containerResponse.Name.ShouldBe(newContainer.Name);
        containerResponse.Description.ShouldBe(newContainer.Description);
        containerResponse.Volume.ShouldBe(newContainer.Volume);
        containerResponse.IsClosed.ShouldBe(newContainer.IsClosed);
        containerResponse.ImageUrl.ShouldBe(newContainer.ImageUrl);
        containerResponse.ImageIsometricUrl.ShouldBe(newContainer.ImageIsometricUrl);
    }

    [Fact]
    public async Task AddContainer_NameTooLong_ShouldReturnBadRequest()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        var longName = _faker.Random.String2(256); // Max is 255
        var newContainer = new ContainersEndpoints.Add.AddContainerRequest
        {
            Name = longName,
            Volume = 5.0f,
            IsClosed = false
        };

        // Act
        var response = await client.PostAsJsonAsync(ApiEndpoints.Containers.Add, newContainer, cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task AddContainer_DuplicateNameDifferentCasing_ShouldReturnBadRequest()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        var baseContainerName = $"{_faker.Lorem.Word()}-{Guid.NewGuid():N}";
        var containerNameLower = baseContainerName.ToLowerInvariant();

        var firstRequest = new ContainersEndpoints.Add.AddContainerRequest
        {
            Name = containerNameLower,
            Volume = 5.0f,
            IsClosed = false
        };
        var secondRequest = new ContainersEndpoints.Add.AddContainerRequest
        {
            Name = containerNameLower.ToUpperInvariant(),
            Volume = 10.0f,
            IsClosed = true
        };

        // Act - Add the container with lowercase
        var firstResponse = await client.PostAsJsonAsync(ApiEndpoints.Containers.Add, firstRequest, cts);
        firstResponse.StatusCode.ShouldBe(HttpStatusCode.Created);

        // Act - Try to add with uppercase
        var secondResponse = await client.PostAsJsonAsync(ApiEndpoints.Containers.Add, secondRequest, cts);

        // Assert
        secondResponse.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task AddContainer_NameWithWhitespace_ShouldTrimAndReturnCreated()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        var containerName = $"{_faker.Lorem.Word()}-{Guid.NewGuid():N}";
        var newContainer = new ContainersEndpoints.Add.AddContainerRequest
        {
            Name = $"  {containerName}  ",
            Volume = 5.0f,
            IsClosed = false
        };

        // Act
        var response = await client.PostAsJsonAsync(ApiEndpoints.Containers.Add, newContainer, cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        var containerResponse = await response.Content.ReadFromJsonAsync<ContainerResponse>(cancellationToken: cts);
        containerResponse.ShouldNotBeNull();
        containerResponse.Name.ShouldBe(containerName);
    }

    [Fact]
    public async Task AddContainer_DescriptionTooLong_ShouldReturnBadRequest()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        var longDescription = _faker.Random.String2(501); // Max is 500
        var newContainer = new ContainersEndpoints.Add.AddContainerRequest
        {
            Name = $"{_faker.Lorem.Word()}-{Guid.NewGuid():N}",
            Description = longDescription,
            Volume = 5.0f,
            IsClosed = false
        };

        // Act
        var response = await client.PostAsJsonAsync(ApiEndpoints.Containers.Add, newContainer, cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task AddContainer_ImageUrlTooLong_ShouldReturnBadRequest()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        var longUrl = $"https://example.com/{_faker.Random.String2(2049)}"; // Max is 2048
        var newContainer = new ContainersEndpoints.Add.AddContainerRequest
        {
            Name = $"{_faker.Lorem.Word()}-{Guid.NewGuid():N}",
            Volume = 5.0f,
            IsClosed = false,
            ImageUrl = longUrl
        };

        // Act
        var response = await client.PostAsJsonAsync(ApiEndpoints.Containers.Add, newContainer, cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task AddContainer_ImageIsometricUrlTooLong_ShouldReturnBadRequest()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        var longUrl = $"https://example.com/{_faker.Random.String2(2049)}"; // Max is 2048
        var newContainer = new ContainersEndpoints.Add.AddContainerRequest
        {
            Name = $"{_faker.Lorem.Word()}-{Guid.NewGuid():N}",
            Volume = 5.0f,
            IsClosed = false,
            ImageIsometricUrl = longUrl
        };

        // Act
        var response = await client.PostAsJsonAsync(ApiEndpoints.Containers.Add, newContainer, cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Theory]
    [InlineData("Small Container")]
    [InlineData("Large Container 123")]
    [InlineData("Container-A1")]
    [InlineData("Premium Container Pro")]
    public async Task AddContainer_ValidNames_ShouldReturnCreated(string containerName)
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        var uniqueName = $"{containerName}-{Guid.NewGuid():N}";
        var newContainer = new ContainersEndpoints.Add.AddContainerRequest
        {
            Name = uniqueName,
            Volume = 5.0f,
            IsClosed = false
        };

        // Act
        var response = await client.PostAsJsonAsync(ApiEndpoints.Containers.Add, newContainer, cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        var containerResponse = await response.Content.ReadFromJsonAsync<ContainerResponse>(cancellationToken: cts);
        containerResponse.ShouldNotBeNull();
        containerResponse.Name.ShouldBe(uniqueName);
    }

    [Theory]
    [InlineData(0.1f)]
    [InlineData(1.0f)]
    [InlineData(50.5f)]
    [InlineData(999.9f)]
    public async Task AddContainer_ValidVolumes_ShouldReturnCreated(float volume)
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        var newContainer = new ContainersEndpoints.Add.AddContainerRequest
        {
            Name = $"{_faker.Lorem.Word()}-{Guid.NewGuid():N}",
            Volume = volume,
            IsClosed = false
        };

        // Act
        var response = await client.PostAsJsonAsync(ApiEndpoints.Containers.Add, newContainer, cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        var containerResponse = await response.Content.ReadFromJsonAsync<ContainerResponse>(cancellationToken: cts);
        containerResponse.ShouldNotBeNull();
        containerResponse.Volume.ShouldBe(volume);
    }
}