using System.Net.Http.Json;
using Bogus;
using Prickle.Api.Endpoints;
using Prickle.Api.Endpoints.Soil;
using Prickle.Application.Soil.Types;

namespace Prickle.Integration.Tests.Api.SoilTypeEndpointTests;

[Collection(nameof(ApiTestsCollectionMarker))]
public sealed class DeleteSoilTypeEndpointTest : ApiBaseIntegrationTest
{
    private readonly Faker _faker;

    public DeleteSoilTypeEndpointTest(AppHostFixture appHostFactory) : base(appHostFactory)
    {
        _faker = new Faker();
    }

    [Fact]
    public async Task DeleteSoilType_ExistingId_ShouldReturnNoContent()
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
        var url = ApiEndpoints.Soil.Delete.Replace("{id:int}", createdSoilType!.Id.ToString());
        var response = await client.DeleteAsync(url, cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DeleteSoilType_NonExistentId_ShouldReturnBadRequest()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");
        var nonExistentId = _faker.Random.Int(999999, 9999999);

        // Act
        var url = ApiEndpoints.Soil.Delete.Replace("{id:int}", nonExistentId.ToString());
        var response = await client.DeleteAsync(url, cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public async Task DeleteSoilType_InvalidId_ShouldReturnBadRequest(int invalidId)
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        // Act
        var url = ApiEndpoints.Soil.Delete.Replace("{id:int}", invalidId.ToString());
        var response = await client.DeleteAsync(url, cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task DeleteSoilType_AfterDeletion_GetShouldReturnBadRequest()
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

        // Act - Delete the soil type
        var deleteUrl = ApiEndpoints.Soil.Delete.Replace("{id:int}", createdSoilType!.Id.ToString());
        var deleteResponse = await client.DeleteAsync(deleteUrl, cts);

        // Act - Try to get the deleted soil type
        var getUrl = ApiEndpoints.Soil.Get.Replace("{id:int}", createdSoilType.Id.ToString());
        var getResponse = await client.GetAsync(getUrl, cts);

        // Assert
        deleteResponse.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        getResponse.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task DeleteSoilType_DeleteTwice_SecondShouldReturnBadRequest()
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

        // Act - Delete the soil type first time
        var url = ApiEndpoints.Soil.Delete.Replace("{id:int}", createdSoilType!.Id.ToString());
        var firstDeleteResponse = await client.DeleteAsync(url, cts);

        // Act - Try to delete the same soil type again
        var secondDeleteResponse = await client.DeleteAsync(url, cts);

        // Assert
        firstDeleteResponse.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        secondDeleteResponse.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task DeleteSoilType_AfterDeletion_CanCreateWithSameName()
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

        // Act - Delete the soil type
        var deleteUrl = ApiEndpoints.Soil.Delete.Replace("{id:int}", createdSoilType!.Id.ToString());
        var deleteResponse = await client.DeleteAsync(deleteUrl, cts);

        // Act - Create a new soil type with the same name
        var recreateResponse = await client.PostAsJsonAsync(ApiEndpoints.Soil.Add, createRequest, cts);

        // Assert
        deleteResponse.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        recreateResponse.StatusCode.ShouldBe(HttpStatusCode.Created);

        var recreatedSoilType = await recreateResponse.Content.ReadFromJsonAsync<SoilTypeResponse>(cancellationToken: cts);
        recreatedSoilType.ShouldNotBeNull();
        recreatedSoilType.Name.ShouldBe(soilTypeName);
        recreatedSoilType.Id.ShouldNotBe(createdSoilType.Id); // Should have a different ID
    }

    [Fact]
    public async Task DeleteSoilType_MultipleDifferentSoilTypes_ShouldDeleteOnlySpecified()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        // Create two soil types
        var firstSoilTypeName = $"{_faker.Lorem.Word()}-{Guid.NewGuid():N}";
        var secondSoilTypeName = $"{_faker.Lorem.Word()}-{Guid.NewGuid():N}";

        var firstCreateResponse = await client.PostAsJsonAsync(ApiEndpoints.Soil.Add,
            new Add.AddSoilTypeRequest(firstSoilTypeName), cts);
        var firstCreatedSoilType = await firstCreateResponse.Content.ReadFromJsonAsync<SoilTypeResponse>(cancellationToken: cts);

        var secondCreateResponse = await client.PostAsJsonAsync(ApiEndpoints.Soil.Add,
            new Add.AddSoilTypeRequest(secondSoilTypeName), cts);
        var secondCreatedSoilType = await secondCreateResponse.Content.ReadFromJsonAsync<SoilTypeResponse>(cancellationToken: cts);

        // Act - Delete only the first soil type
        var deleteUrl = ApiEndpoints.Soil.Delete.Replace("{id:int}", firstCreatedSoilType!.Id.ToString());
        var deleteResponse = await client.DeleteAsync(deleteUrl, cts);

        // Act - Try to get both soil types
        var firstGetUrl = ApiEndpoints.Soil.Get.Replace("{id:int}", firstCreatedSoilType.Id.ToString());
        var secondGetUrl = ApiEndpoints.Soil.Get.Replace("{id:int}", secondCreatedSoilType!.Id.ToString());

        var firstGetResponse = await client.GetAsync(firstGetUrl, cts);
        var secondGetResponse = await client.GetAsync(secondGetUrl, cts);

        // Assert
        deleteResponse.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        firstGetResponse.StatusCode.ShouldBe(HttpStatusCode.BadRequest); // First one was deleted
        secondGetResponse.StatusCode.ShouldBe(HttpStatusCode.OK); // Second one still exists

        var retrievedSecondSoilType = await secondGetResponse.Content.ReadFromJsonAsync<SoilTypeResponse>(cancellationToken: cts);
        retrievedSecondSoilType.ShouldNotBeNull();
        retrievedSecondSoilType.Id.ShouldBe(secondCreatedSoilType.Id);
        retrievedSecondSoilType.Name.ShouldBe(secondSoilTypeName);
    }

    [Fact]
    public async Task DeleteSoilType_AfterDeletion_UpdateShouldReturnBadRequest()
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

        // Act - Delete the soil type
        var deleteUrl = ApiEndpoints.Soil.Delete.Replace("{id:int}", createdSoilType!.Id.ToString());
        var deleteResponse = await client.DeleteAsync(deleteUrl, cts);

        // Act - Try to update the deleted soil type
        var newName = $"{_faker.Lorem.Word()}-{Guid.NewGuid():N}";
        var updateRequest = new Update.UpdateSoilTypeRequest(newName);
        var updateUrl = ApiEndpoints.Soil.Update.Replace("{id:int}", createdSoilType.Id.ToString());
        var updateResponse = await client.PatchAsJsonAsync(updateUrl, updateRequest, cts);

        // Assert
        deleteResponse.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        updateResponse.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task DeleteSoilType_CreatedAndImmediatelyDeleted_ShouldSucceed()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        // Act - Create and immediately delete
        var soilTypeName = $"{_faker.Lorem.Word()}-{Guid.NewGuid():N}";
        var createRequest = new Add.AddSoilTypeRequest(soilTypeName);
        var createResponse = await client.PostAsJsonAsync(ApiEndpoints.Soil.Add, createRequest, cts);
        var createdSoilType = await createResponse.Content.ReadFromJsonAsync<SoilTypeResponse>(cancellationToken: cts);

        var deleteUrl = ApiEndpoints.Soil.Delete.Replace("{id:int}", createdSoilType!.Id.ToString());
        var deleteResponse = await client.DeleteAsync(deleteUrl, cts);

        // Assert
        createResponse.StatusCode.ShouldBe(HttpStatusCode.Created);
        deleteResponse.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DeleteSoilType_AfterUpdate_ShouldDeleteSuccessfully()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        // Create a soil type
        var originalName = $"{_faker.Lorem.Word()}-{Guid.NewGuid():N}";
        var createRequest = new Add.AddSoilTypeRequest(originalName);
        var createResponse = await client.PostAsJsonAsync(ApiEndpoints.Soil.Add, createRequest, cts);
        var createdSoilType = await createResponse.Content.ReadFromJsonAsync<SoilTypeResponse>(cancellationToken: cts);

        // Update the soil type
        var newName = $"{_faker.Lorem.Word()}-{Guid.NewGuid():N}";
        var updateRequest = new Update.UpdateSoilTypeRequest(newName);
        var updateUrl = ApiEndpoints.Soil.Update.Replace("{id:int}", createdSoilType!.Id.ToString());
        var updateResponse = await client.PatchAsJsonAsync(updateUrl, updateRequest, cts);

        // Act - Delete the updated soil type
        var deleteUrl = ApiEndpoints.Soil.Delete.Replace("{id:int}", createdSoilType.Id.ToString());
        var deleteResponse = await client.DeleteAsync(deleteUrl, cts);

        // Assert
        updateResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        deleteResponse.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        // Verify deletion
        var getUrl = ApiEndpoints.Soil.Get.Replace("{id:int}", createdSoilType.Id.ToString());
        var getResponse = await client.GetAsync(getUrl, cts);
        getResponse.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }
}
