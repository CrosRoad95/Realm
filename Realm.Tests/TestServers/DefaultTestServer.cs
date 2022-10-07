namespace Realm.Tests.TestServers;

internal class DefaultTestServer
{
    public MtaServer<TestRPGPlayer> TestServer { get; private set; }

    public DefaultTestServer()
    {
        var configuration = new TestConfiguration().Configuration;
        TestServer = MtaServer.CreateWithDiSupport<TestRPGPlayer>(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.AddSingleton<IConsoleCommands, TestConsoleCommands>();
            });
            builder.ConfigureServer(configuration);
        });
    }
}
