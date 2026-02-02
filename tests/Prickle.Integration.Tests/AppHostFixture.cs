using Aspire.Hosting;
namespace Prickle.Integration.Tests;

public class AppHostFixture : IAsyncLifetime
{
    private static readonly TimeSpan BuildStopTimeout = TimeSpan.FromSeconds(60);
    public static readonly TimeSpan StartStopTimeout = TimeSpan.FromSeconds(120);

    private DistributedApplication? _app;

    public DistributedApplication App => _app ?? throw new InvalidOperationException("App not initialized");

    public async ValueTask InitializeAsync()
    {
        var appHost = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.Prickle_AppHost>();

        _app = await appHost.BuildAsync()
            .WaitAsync(BuildStopTimeout);
    }
    public async ValueTask DisposeAsync()
    {
        if (_app is not null)
        {
            await _app.DisposeAsync();
        }
    }
}
