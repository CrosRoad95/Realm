namespace Realm.Tests.TestServers;

internal class DefaultTestServer
{
    public MtaServer<TestRPGPlayer> TestServer { get; private set; }

    public DefaultTestServer()
    {
        var configuration = new TestConfiguration().Configuration;
        Console.WriteLine("Files: {0}", string.Join(' ', Directory.GetFiles(".")));
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
