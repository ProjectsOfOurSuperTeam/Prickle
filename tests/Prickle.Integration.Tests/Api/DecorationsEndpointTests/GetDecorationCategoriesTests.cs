using System.Net.Http.Json;
using Prickle.Api.Endpoints;
using Prickle.Application.Decorations.GetCategories;
using Prickle.Domain.Decorations;

namespace Prickle.Integration.Tests.Api.DecorationsEndpointTests;

[Collection(nameof(ApiTestsCollectionMarker))]
public sealed class GetDecorationCategoriesTests : ApiBaseIntegrationTest
{
    public GetDecorationCategoriesTests(AppHostFixture appHostFactory) : base(appHostFactory)
    {
    }

    [Fact]
    public async Task GetDecorationCategories_ReturnsOk()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        // Act
        var response = await client.GetAsync(ApiEndpoints.Decorations.GetCategories, cts);
        var result = await response.Content.ReadFromJsonAsync<DecorationCategoriesResponse>(cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        result.ShouldNotBeNull();
    }

    [Fact]
    public async Task GetDecorationCategories_ReturnsAllCategories()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        // Act
        var response = await client.GetAsync(ApiEndpoints.Decorations.GetCategories, cts);
        var result = await response.Content.ReadFromJsonAsync<DecorationCategoriesResponse>(cts);

        // Assert
        result.Items.Count().ShouldBe(DecorationCategory.List.Count);
    }

    [Fact]
    public async Task GetDecorationCategories_ReturnsExpectedCategoryValues()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        // Act
        var response = await client.GetAsync(ApiEndpoints.Decorations.GetCategories, cts);
        var result = await response.Content.ReadFromJsonAsync<DecorationCategoriesResponse>(cts);

        // Assert
        var categories = result.Items.ToList();
        categories.ShouldContain(c => c.Name == "NoCategory" && c.Id == 0);
        categories.ShouldContain(c => c.Name == "Stones" && c.Id == 1);
        categories.ShouldContain(c => c.Name == "Sand" && c.Id == 2);
        categories.ShouldContain(c => c.Name == "Figurines" && c.Id == 3);
    }

    [Fact]
    public async Task GetDecorationCategories_ResponseItemsAreNotNull()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        // Act
        var response = await client.GetAsync(ApiEndpoints.Decorations.GetCategories, cts);
        var result = await response.Content.ReadFromJsonAsync<DecorationCategoriesResponse>(cts);

        // Assert
        result.Items.ShouldNotBeNull();
        result.Items.ShouldNotBeEmpty();
        result.Items.ShouldAllBe(item => item.Name != null && item.Name.Length > 0);
    }

    [Fact]
    public async Task GetDecorationCategories_ReturnsCorrectContentType()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        // Act
        var response = await client.GetAsync(ApiEndpoints.Decorations.GetCategories, cts);

        // Assert
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/json");
    }
}