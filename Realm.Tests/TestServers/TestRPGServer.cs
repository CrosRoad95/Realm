namespace Realm.Tests.TestServers;

internal class TestRPGServer : IReloadable, IRPGServer
{
    public MtaServer<TestRPGPlayer> TestServer { get; private set; }
    public event Action<IRPGPlayer>? PlayerJoined;

    public TestRPGServer()
    {
        var configuration = new TestConfiguration().Configuration;
        TestServer = MtaServer.CreateWithDiSupport<TestRPGPlayer>(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.AddSingleton<IConsoleCommands, TestConsoleCommands>();
                services.AddSingleton<IReloadable>(this);
                services.AddSingleton<IRPGServer>(this);
                services.AddSingleton(new Logger().GetLogger().ForContext<IRPGServer>());
            });
            builder.ConfigureServer(configuration);
        });
        TestServer.PlayerJoined += e => PlayerJoined?.Invoke(e);
    }

    public void Reload()
    {
    }

    public int GetPriority() => int.MaxValue;
}
