using Realm.Server.Interfaces;
using SlipeServer.Server.Elements;

namespace Realm.Tests.TestServers;

internal class TestRPGServer : IReloadable, IRPGServer
{
    public MtaServer<TestRPGPlayer> TestServer { get; private set; }

    public event Action<Player>? PlayerJoined;

    public TestRPGServer()
    {
        var configuration = new TestConfiguration().Configuration;
        TestServer = MtaServer.CreateWithDiSupport<TestRPGPlayer>(builder =>
        {
            builder.AddGuiFilesLocation("Gui");
            builder.ConfigureServices(services =>
            {
                services.AddSingleton(new Configuration.ConfigurationProvider());
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

    public TService GetRequiredService<TService>() where TService : notnull
    {
        return TestServer.GetRequiredService<TService>();
    }

    public void SubscribeLuaEvent(string eventName, Func<LuaEventContext, Task> callback)
    {
        //throw new NotImplementedException();
    }

    public void AssociateElement(Element element)
    {
        //throw new NotImplementedException();
    }
}
