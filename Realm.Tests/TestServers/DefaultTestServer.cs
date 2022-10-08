namespace Realm.Tests.TestServers;

internal class DefaultTestServer : IReloadable, IMtaServer
{
    public MtaServer<TestRPGPlayer> TestServer { get; private set; }
    public event Action<IRPGPlayer>? PlayerJoined;

    public DefaultTestServer()
    {
        var configuration = new TestConfiguration().Configuration;
        TestServer = MtaServer.CreateWithDiSupport<TestRPGPlayer>(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.AddSingleton<IConsoleCommands, TestConsoleCommands>();
                services.AddSingleton<IReloadable>(this);
                services.AddSingleton<IMtaServer>(this);
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
