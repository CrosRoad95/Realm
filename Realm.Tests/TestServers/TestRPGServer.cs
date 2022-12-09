using Realm.Discord;
using Realm.Interfaces.Common;
using Realm.Interfaces.Extend;
using Realm.Interfaces.Server;
using Realm.Logging;
using Realm.Server.Extensions;
using Realm.Server.Factories;
using Realm.Server.Interfaces;
using Realm.Server.Modules;
using Realm.Tests.Modules;
using Realm.WebApp;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Events;

namespace Realm.Tests.TestServers;

internal class TestRPGServer : IReloadable, IRPGServer
{
    public MtaServer<TestRPGPlayer> TestServer { get; private set; }
    public string MapName { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public string GameType { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public event Action<Player>? PlayerJoined;
    public event Action? ServerReloaded;

    public TestRPGServer()
    {
        var modules = new IModule[]
        {
            new DiscordModule(),
            new IdentityModule(),
            new WebPanelModule(new TestWebPanelIntegration()),
            new ScriptingModule(),
            new ServerScriptingModule(),
        };

        var configuration = new TestConfigurationProvider();
        TestServer = MtaServer.CreateWithDiSupport<TestRPGPlayer>(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.AddSingleton(configuration);
                services.AddSingleton<IConsole, TestConsoleCommands>();
                services.AddSingleton<IReloadable>(this);
                services.AddSingleton<IRPGServer>(this);
                services.AddSingleton(new RealmLogger().GetLogger());

                services.AddSingleton<RPGElementsFactory>();

                if (modules != null)
                    foreach (var module in modules)
                    {
                        services.AddSingleton(module);
                        module.Configure(services);
                    }
            });
            builder.ConfigureServer(configuration);
        });
        TestServer.PlayerJoined += e => PlayerJoined?.Invoke(e);
    }

    public Task Reload()
    {
        return Task.CompletedTask;
    }

    public int GetPriority() => int.MaxValue;

    public TService GetRequiredService<TService>() where TService : notnull
    {
        return TestServer.GetRequiredService<TService>();
    }

    public void AssociateElement(Element element)
    {
        //throw new NotImplementedException();
    }

    public void AddEventHandler(string eventName, Func<LuaEvent, Task<object?>> callback)
    {
        //throw new NotImplementedException();
    }
}
