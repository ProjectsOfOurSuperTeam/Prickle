using System.Net.Http.Json;
using Bogus;
using Prickle.Api.Endpoints;
using Prickle.Application.Soil.Formulas;
using Prickle.Application.Soil.Types;
using SoilFormulaEndpoints = Prickle.Api.Endpoints.Soil.Formulas;
using SoilTypeEndpoints = Prickle.Api.Endpoints.Soil.Types;

namespace Prickle.Integration.Tests.Api.SoilFormulaEndpointTests;

[Collection(nameof(ApiTestsCollectionMarker))]
public sealed class DeleteSoilFormulaEndpointTest : ApiBaseIntegrationTest
{
    private readonly Faker _faker;

    public DeleteSoilFormulaEndpointTest(AppHostFixture appHostFactory) : base(appHostFactory)
    {
        _faker = new Faker();
    }

    [Fact]
    public async Task DeleteSoilFormula_ExistingId_ShouldReturnNoContent()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        // Create a soil formula first
        var soilType = await CreateSoilType(client, cts);
        var createdFormula = await CreateSoilFormula(client, soilType, cts);

        // Act
        var url = ApiEndpoints.Soil.Formulas.Delete.Replace("{id:guid}", createdFormula.Id.ToString());
        var response = await client.DeleteAsync(url, cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DeleteSoilFormula_NonExistentId_ShouldReturnBadRequest()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");
        var nonExistentId = Guid.NewGuid();

        // Act
        var url = ApiEndpoints.Soil.Formulas.Delete.Replace("{id:guid}", nonExistentId.ToString());
        var response = await client.DeleteAsync(url, cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task DeleteSoilFormula_EmptyGuid_ShouldReturnBadRequest()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        // Act
        var url = ApiEndpoints.Soil.Formulas.Delete.Replace("{id:guid}", Guid.Empty.ToString());
        var response = await client.DeleteAsync(url, cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task DeleteSoilFormula_DeleteTwice_SecondShouldReturnBadRequest()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        // Create a soil formula
        var soilType = await CreateSoilType(client, cts);
        var createdFormula = await CreateSoilFormula(client, soilType, cts);

        // Act - Delete the soil formula first time
        var url = ApiEndpoints.Soil.Formulas.Delete.Replace("{id:guid}", createdFormula.Id.ToString());
        var firstDeleteResponse = await client.DeleteAsync(url, cts);

        // Act - Try to delete the same soil formula again
        var secondDeleteResponse = await client.DeleteAsync(url, cts);

        // Assert
        firstDeleteResponse.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        secondDeleteResponse.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task DeleteSoilFormula_AfterDeletion_CanCreateWithSameName()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        var soilType = await CreateSoilType(client, cts);

        // Create a soil formula
        var formulaName = $"{_faker.Lorem.Word()}-{Guid.NewGuid():N}";
        var formulaItems = new List<SoilFormulaItemDTO> { new(soilType.Id, 100, 0) };
        var createRequest = new SoilFormulaEndpoints.Add.AddSoilFormulaRequest(formulaName, formulaItems);
        var createResponse = await client.PostAsJsonAsync(ApiEndpoints.Soil.Formulas.Add, createRequest, cts);
        var createdFormula = await createResponse.Content.ReadFromJsonAsync<SoilFormulaResponse>(cancellationToken: cts);

        // Act - Delete the soil formula
        var deleteUrl = ApiEndpoints.Soil.Formulas.Delete.Replace("{id:guid}", createdFormula!.Id.ToString());
        var deleteResponse = await client.DeleteAsync(deleteUrl, cts);

        // Act - Create a new soil formula with the same name
        var recreateResponse = await client.PostAsJsonAsync(ApiEndpoints.Soil.Formulas.Add, createRequest, cts);

        // Assert
        deleteResponse.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        recreateResponse.StatusCode.ShouldBe(HttpStatusCode.Created);

        var recreatedFormula = await recreateResponse.Content.ReadFromJsonAsync<SoilFormulaResponse>(cancellationToken: cts);
        recreatedFormula.ShouldNotBeNull();
        recreatedFormula.Name.ShouldBe(formulaName);
        recreatedFormula.Id.ShouldNotBe(createdFormula.Id); // Should have a different ID
    }

    [Fact]
    public async Task DeleteSoilFormula_WithMultipleItems_ShouldDeleteAllItems()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        var soilType1 = await CreateSoilType(client, cts);
        var soilType2 = await CreateSoilType(client, cts);
        var soilType3 = await CreateSoilType(client, cts);

        // Create a soil formula with multiple items
        var formulaName = $"{_faker.Lorem.Word()}-{Guid.NewGuid():N}";
        var formulaItems = new List<SoilFormulaItemDTO>
        {
            new(soilType1.Id, 50, 0),
            new(soilType2.Id, 30, 1),
            new(soilType3.Id, 20, 2)
        };
        var createRequest = new SoilFormulaEndpoints.Add.AddSoilFormulaRequest(formulaName, formulaItems);
        var createResponse = await client.PostAsJsonAsync(ApiEndpoints.Soil.Formulas.Add, createRequest, cts);
        var createdFormula = await createResponse.Content.ReadFromJsonAsync<SoilFormulaResponse>(cancellationToken: cts);

        // Act - Delete the soil formula
        var deleteUrl = ApiEndpoints.Soil.Formulas.Delete.Replace("{id:guid}", createdFormula!.Id.ToString());
        var deleteResponse = await client.DeleteAsync(deleteUrl, cts);

        // Assert
        deleteResponse.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DeleteSoilFormula_DeleteMultipleFormulas_ShouldSucceed()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        var soilType = await CreateSoilType(client, cts);

        // Create three soil formulas
        var formula1 = await CreateSoilFormula(client, soilType, cts, $"Formula1-{Guid.NewGuid():N}");
        var formula2 = await CreateSoilFormula(client, soilType, cts, $"Formula2-{Guid.NewGuid():N}");
        var formula3 = await CreateSoilFormula(client, soilType, cts, $"Formula3-{Guid.NewGuid():N}");

        // Act - Delete all three formulas
        var url1 = ApiEndpoints.Soil.Formulas.Delete.Replace("{id:guid}", formula1.Id.ToString());
        var url2 = ApiEndpoints.Soil.Formulas.Delete.Replace("{id:guid}", formula2.Id.ToString());
        var url3 = ApiEndpoints.Soil.Formulas.Delete.Replace("{id:guid}", formula3.Id.ToString());

        var response1 = await client.DeleteAsync(url1, cts);
        var response2 = await client.DeleteAsync(url2, cts);
        var response3 = await client.DeleteAsync(url3, cts);

        // Assert
        response1.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        response2.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        response3.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DeleteSoilFormula_DeletedFormula_CannotBeUpdated()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        var soilType = await CreateSoilType(client, cts);

        // Create a soil formula
        var createdFormula = await CreateSoilFormula(client, soilType, cts);

        // Act - Delete the soil formula
        var deleteUrl = ApiEndpoints.Soil.Formulas.Delete.Replace("{id:guid}", createdFormula.Id.ToString());
        var deleteResponse = await client.DeleteAsync(deleteUrl, cts);

        // Act - Try to update the deleted soil formula
        var newName = $"{_faker.Lorem.Word()}-{Guid.NewGuid():N}";
        var formulaItems = new List<SoilFormulaItemDTO> { new(soilType.Id, 100, 0) };
        var updateRequest = new SoilFormulaEndpoints.Update.UpdateSoilFormulaRequest(newName, formulaItems);
        var updateUrl = ApiEndpoints.Soil.Formulas.Update.Replace("{id:guid}", createdFormula.Id.ToString());
        var updateResponse = await client.PatchAsJsonAsync(updateUrl, updateRequest, cts);

        // Assert
        deleteResponse.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        updateResponse.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task DeleteSoilFormula_AfterCreationAndUpdate_ShouldDeleteSuccessfully()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        var soilType1 = await CreateSoilType(client, cts);
        var soilType2 = await CreateSoilType(client, cts);

        // Create a soil formula
        var originalName = $"{_faker.Lorem.Word()}-{Guid.NewGuid():N}";
        var formulaItems = new List<SoilFormulaItemDTO> { new(soilType1.Id, 100, 0) };
        var createRequest = new SoilFormulaEndpoints.Add.AddSoilFormulaRequest(originalName, formulaItems);
        var createResponse = await client.PostAsJsonAsync(ApiEndpoints.Soil.Formulas.Add, createRequest, cts);
        var createdFormula = await createResponse.Content.ReadFromJsonAsync<SoilFormulaResponse>(cancellationToken: cts);

        // Update the formula
        var updatedName = $"{_faker.Lorem.Word()}-{Guid.NewGuid():N}";
        var updatedFormulaItems = new List<SoilFormulaItemDTO>
        {
            new(soilType1.Id, 60, 0),
            new(soilType2.Id, 40, 1)
        };
        var updateRequest = new SoilFormulaEndpoints.Update.UpdateSoilFormulaRequest(updatedName, updatedFormulaItems);
        var updateUrl = ApiEndpoints.Soil.Formulas.Update.Replace("{id:guid}", createdFormula!.Id.ToString());
        var updateResponse = await client.PatchAsJsonAsync(updateUrl, updateRequest, cts);

        // Act - Delete the soil formula
        var deleteUrl = ApiEndpoints.Soil.Formulas.Delete.Replace("{id:guid}", createdFormula.Id.ToString());
        var deleteResponse = await client.DeleteAsync(deleteUrl, cts);

        // Assert
        createResponse.StatusCode.ShouldBe(HttpStatusCode.Created);
        updateResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        deleteResponse.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DeleteSoilFormula_DoesNotAffectOtherFormulas()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        var soilType = await CreateSoilType(client, cts);

        // Create two soil formulas
        var formula1 = await CreateSoilFormula(client, soilType, cts, $"Formula1-{Guid.NewGuid():N}");
        var formula2 = await CreateSoilFormula(client, soilType, cts, $"Formula2-{Guid.NewGuid():N}");

        // Act - Delete only the first formula
        var deleteUrl = ApiEndpoints.Soil.Formulas.Delete.Replace("{id:guid}", formula1.Id.ToString());
        var deleteResponse = await client.DeleteAsync(deleteUrl, cts);

        // Assert
        deleteResponse.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        // Verify second formula still exists by attempting to update it
        var newName = $"{_faker.Lorem.Word()}-{Guid.NewGuid():N}";
        var formulaItems = new List<SoilFormulaItemDTO> { new(soilType.Id, 100, 0) };
        var updateRequest = new SoilFormulaEndpoints.Update.UpdateSoilFormulaRequest(newName, formulaItems);
        var updateUrl = ApiEndpoints.Soil.Formulas.Update.Replace("{id:guid}", formula2.Id.ToString());
        var updateResponse = await client.PatchAsJsonAsync(updateUrl, updateRequest, cts);

        updateResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task DeleteSoilFormula_WithComplexFormula_ShouldDeleteAllRelatedData()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        var soilType1 = await CreateSoilType(client, cts);
        var soilType2 = await CreateSoilType(client, cts);
        var soilType3 = await CreateSoilType(client, cts);
        var soilType4 = await CreateSoilType(client, cts);

        // Create a complex soil formula with multiple items
        var formulaName = $"Complex-{Guid.NewGuid():N}";
        var formulaItems = new List<SoilFormulaItemDTO>
        {
            new(soilType1.Id, 40, 0),
            new(soilType2.Id, 25, 1),
            new(soilType3.Id, 20, 2),
            new(soilType4.Id, 15, 3)
        };
        var createRequest = new SoilFormulaEndpoints.Add.AddSoilFormulaRequest(formulaName, formulaItems);
        var createResponse = await client.PostAsJsonAsync(ApiEndpoints.Soil.Formulas.Add, createRequest, cts);
        createResponse.StatusCode.ShouldBe(HttpStatusCode.Created);
        var createdFormula = await createResponse.Content.ReadFromJsonAsync<SoilFormulaResponse>(cancellationToken: cts);

        // Act - Delete the complex soil formula
        var deleteUrl = ApiEndpoints.Soil.Formulas.Delete.Replace("{id:guid}", createdFormula!.Id.ToString());
        var deleteResponse = await client.DeleteAsync(deleteUrl, cts);

        // Assert
        deleteResponse.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        // Verify formula is gone by trying to update it
        var updateRequest = new SoilFormulaEndpoints.Update.UpdateSoilFormulaRequest($"Updated-{Guid.NewGuid():N}", formulaItems);
        var updateUrl = ApiEndpoints.Soil.Formulas.Update.Replace("{id:guid}", createdFormula.Id.ToString());
        var updateResponse = await client.PatchAsJsonAsync(updateUrl, updateRequest, cts);
        updateResponse.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task DeleteSoilFormula_DoesNotDeleteReferencedSoilTypes()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        var soilType = await CreateSoilType(client, cts);

        // Create a soil formula using the soil type
        var createdFormula = await CreateSoilFormula(client, soilType, cts);

        // Act - Delete the soil formula
        var deleteUrl = ApiEndpoints.Soil.Formulas.Delete.Replace("{id:guid}", createdFormula.Id.ToString());
        var deleteResponse = await client.DeleteAsync(deleteUrl, cts);

        // Assert - Formula is deleted
        deleteResponse.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        // Verify the soil type still exists
        var getUrl = ApiEndpoints.Soil.Types.Get.Replace("{id:int}", soilType.Id.ToString());
        var getResponse = await client.GetAsync(getUrl, cts);
        getResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        var retrievedSoilType = await getResponse.Content.ReadFromJsonAsync<SoilTypeResponse>(cancellationToken: cts);
        retrievedSoilType.ShouldNotBeNull();
        retrievedSoilType.Id.ShouldBe(soilType.Id);
        retrievedSoilType.Name.ShouldBe(soilType.Name);
    }

    [Theory]
    [InlineData("Formula-A")]
    [InlineData("Complex Blend")]
    [InlineData("Test Mix 123")]
    public async Task DeleteSoilFormula_VariousFormulaNames_ShouldDeleteSuccessfully(string namePrefix)
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        var soilType = await CreateSoilType(client, cts);

        // Create a soil formula with specific name
        var formulaName = $"{namePrefix}-{Guid.NewGuid():N}";
        var createdFormula = await CreateSoilFormula(client, soilType, cts, formulaName);

        // Act
        var url = ApiEndpoints.Soil.Formulas.Delete.Replace("{id:guid}", createdFormula.Id.ToString());
        var response = await client.DeleteAsync(url, cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DeleteSoilFormula_ConcurrentDeletes_ShouldHandleCorrectly()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        var soilType = await CreateSoilType(client, cts);

        // Create multiple formulas
        var formula1 = await CreateSoilFormula(client, soilType, cts, $"Formula1-{Guid.NewGuid():N}");
        var formula2 = await CreateSoilFormula(client, soilType, cts, $"Formula2-{Guid.NewGuid():N}");
        var formula3 = await CreateSoilFormula(client, soilType, cts, $"Formula3-{Guid.NewGuid():N}");

        // Act - Delete formulas concurrently
        var url1 = ApiEndpoints.Soil.Formulas.Delete.Replace("{id:guid}", formula1.Id.ToString());
        var url2 = ApiEndpoints.Soil.Formulas.Delete.Replace("{id:guid}", formula2.Id.ToString());
        var url3 = ApiEndpoints.Soil.Formulas.Delete.Replace("{id:guid}", formula3.Id.ToString());

        var deleteTask1 = client.DeleteAsync(url1, cts);
        var deleteTask2 = client.DeleteAsync(url2, cts);
        var deleteTask3 = client.DeleteAsync(url3, cts);

        var responses = await Task.WhenAll(deleteTask1, deleteTask2, deleteTask3);

        // Assert
        responses[0].StatusCode.ShouldBe(HttpStatusCode.NoContent);
        responses[1].StatusCode.ShouldBe(HttpStatusCode.NoContent);
        responses[2].StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    // Helper methods
    private async Task<SoilTypeResponse> CreateSoilType(HttpClient client, CancellationToken cancellationToken)
    {
        var soilTypeName = $"{_faker.Lorem.Word()}-{Guid.NewGuid():N}";
        var soilTypeRequest = new SoilTypeEndpoints.Add.AddSoilTypeRequest(soilTypeName);
        var response = await client.PostAsJsonAsync(ApiEndpoints.Soil.Types.Add, soilTypeRequest, cancellationToken);
        response.EnsureSuccessStatusCode();
        var soilType = await response.Content.ReadFromJsonAsync<SoilTypeResponse>(cancellationToken: cancellationToken);
        return soilType!;
    }

    private async Task<SoilFormulaResponse> CreateSoilFormula(HttpClient client, SoilTypeResponse soilType, CancellationToken cancellationToken, string? name = null)
    {
        var formulaName = name ?? $"{_faker.Lorem.Word()}-{Guid.NewGuid():N}";
        var formulaItems = new List<SoilFormulaItemDTO> { new(soilType.Id, 100, 0) };
        var createRequest = new SoilFormulaEndpoints.Add.AddSoilFormulaRequest(formulaName, formulaItems);
        var response = await client.PostAsJsonAsync(ApiEndpoints.Soil.Formulas.Add, createRequest, cancellationToken);
        response.EnsureSuccessStatusCode();
        var formula = await response.Content.ReadFromJsonAsync<SoilFormulaResponse>(cancellationToken: cancellationToken);
        return formula!;
    }
}
