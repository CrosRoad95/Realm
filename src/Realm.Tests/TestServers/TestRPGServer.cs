using Realm.Module.Discord;
using Realm.Interfaces.Extend;
using Realm.Interfaces.Server;
using Realm.Logging;
using Realm.Server.Extensions;
using SlipeServer.Server.Elements;
using Realm.Interfaces.Providers;
using Realm.Server.Providers;
using Realm.Domain;
using Realm.Module.WebApp;

namespace Realm.Tests.TestServers;

internal class TestRPGServer : IRPGServer
{
    public MtaServer<RealmTestingPlayer> TestServer { get; private set; }
    public ECS ECS => throw new NotImplementedException();

    public event Action<Entity>? PlayerJoined;
    public event Action? ServerStarted;

    public TestRPGServer()
    {
        var configuration = new TestConfigurationProvider();
        TestServer = MtaServer.CreateWithDiSupport<RealmTestingPlayer>(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.AddSingleton(configuration);
                services.AddSingleton<IConsole, TestConsoleCommands>();
                services.AddSingleton((IServerFilesProvider)NullServerFilesProvider.Instance);
                services.AddSingleton(new RealmLogger().GetLogger());

                services.AddDiscordModule();
                services.AddWebAppModule();
            });
            builder.ConfigureServer(configuration);
        });
    }

    public TService GetRequiredService<TService>() where TService : notnull
    {
        return TestServer.GetRequiredService<TService>();
    }

    public Entity CreateEntity(string name)
    {
        throw new NotImplementedException();
    }

    public Entity GetEntityByPlayer(Player player)
    {
        throw new NotImplementedException();
    }

    public object GetRequiredService(Type type)
    {
        throw new NotImplementedException();
    }

    public Task Start()
    {
        throw new NotImplementedException();
    }

    public Task Stop()
    {
        throw new NotImplementedException();
    }
}
