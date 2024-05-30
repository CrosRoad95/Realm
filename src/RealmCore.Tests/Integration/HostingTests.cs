namespace RealmCore.Tests.Integration;

public class HostingTests
{
    [Fact]
    public void HostingShouldWork()
    {
        var sampleService = new SampleHostedService();

        RealmTestingPlayer player;
        {
            using var hosting = new RealmTestingServerHosting(hostBuilder =>
            {
                hostBuilder.Services.AddHostedService(x => sampleService);
            }, null);

            player = hosting.CreatePlayer();

            player.Client.IsConnected.Should().BeTrue();
        }

        player.Client.IsConnected.Should().BeFalse();
        sampleService.Started.Should().BeTrue();
        sampleService.Stopped.Should().BeTrue();
    }
}

public class SampleHostedService : IHostedService
{
    public bool Started { get; private set; }
    public bool Stopped { get; private set; }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        Started = true;
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        Stopped = true;
        return Task.CompletedTask;
    }
}