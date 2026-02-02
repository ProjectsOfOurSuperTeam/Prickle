namespace Prickle.Integration.Tests.Api;

[CollectionDefinition(nameof(ApiTestsCollectionMarker))]
public class ApiBaseIntegrationTest : IClassFixture<AppHostFixture>
{
    protected AppHostFixture AppHostFactory { get; }

    protected ApiBaseIntegrationTest(AppHostFixture appHostFactory)
    {
        AppHostFactory = appHostFactory;
    }
    protected HttpClient CreateHttpClient(string resourceName)
    {
        return AppHostFactory.App.CreateHttpClient(resourceName);
    }

    protected HttpClient CreateHttpClient(string resourceName, string endpointName)
    {
        return AppHostFactory.App.CreateHttpClient(resourceName, endpointName);
    }
}
