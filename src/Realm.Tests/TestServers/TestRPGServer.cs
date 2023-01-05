using Realm.Module.Discord;
using Realm.Interfaces.Extend;
using Realm.Interfaces.Server;
using Realm.Logging;
using Realm.Server.Extensions;
using SlipeServer.Server.Elements;
using Realm.Interfaces.Providers;
using Realm.Server.Providers;
using Realm.Domain;
using Realm.Server;

namespace Realm.Tests.TestServers;

internal class TestRPGServer : IRPGServer
{
    public MtaServer<RealmTestingPlayer> TestServer { get; private set; }
    public ECS ECS => throw new NotImplementedException();

    public event Action<Entity>? PlayerJoined;

    public TestRPGServer()
    {
        var modules = new IModule[]
        {
            new DiscordModule(),
        };

        var configuration = new TestConfigurationProvider();
        TestServer = MtaServer.CreateWithDiSupport<RealmTestingPlayer>(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.AddSingleton(configuration);
                services.AddSingleton<IConsole, TestConsoleCommands>();
                services.AddSingleton((IServerFilesProvider)NullServerFilesProvider.Instance);
                services.AddSingleton(new RealmLogger().GetLogger());

                if (modules != null)
                    foreach (var module in modules)
                    {
                        services.AddSingleton(module);
                        module.Configure(services);
                    }
            });
            builder.ConfigureServer(configuration);
        });
    }

    public TService GetRequiredService<TService>() where TService : notnull
    {
        return TestServer.GetRequiredService<TService>();
    }

    public void AssociateElement(Element element)
    {
        //throw new NotImplementedException();
    }

    public Entity CreateEntity(string name)
    {
        throw new NotImplementedException();
    }

    public Entity GetEntityByPlayer(Player player)
    {
        throw new NotImplementedException();
    }

    public void AssociateElement(IElementHandle elementHandle)
    {
        throw new NotImplementedException();
    }

    public object GetRequiredService(Type type)
    {
        throw new NotImplementedException();
    }
}
