using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RealmCore.WebHosting;
using SlipeServer.Hosting;
using SlipeServer.Server;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Mappers;
using SlipeServer.Server.Resources.Serving;
using SlipeServer.Server.ServerBuilders;
using System.Numerics;

namespace RealmCore.TestingTools;

public class RealmTestingServer2<TPlayer> : TestingServer<TPlayer> where TPlayer : Player
{
    public RealmTestingServer2(IServiceProvider serviceProvider, Configuration? configuration = null) : base(serviceProvider, configuration)
    {
    }

    protected override IClient CreateClient(uint binaryAddress, INetWrapper netWrapper)
    {
        var player = Instantiate<TPlayer>();
        player.Name = "foo"; // TODO:
        player.Client = new TestingClient(binaryAddress, netWrapper, player);
        return player.Client;
    }
}

public class RealmTestingServerHosting : TestingServerHosting<RealmTestingPlayer>
{
    public RealmTestingServerHosting(Action<HostApplicationBuilder>? outerApplicationBuilder = null, Action<ServerBuilder>? outerServerBuilder = null) : base(new(), services => new RealmTestingServer2<RealmTestingPlayer>(services, new()), hostBuilder =>
    {
        hostBuilder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Server:IsVoiceEnabled"] = "true",
            ["Server:ServerName"] = "Default New-RealmCore Test server",
            ["Server:Port"] = "22300",
            ["Server:HttpPort"] = "22301",
            ["Server:MaxPlayerCount"] = "128",
            ["ServerList:GameType"] = "",
            ["ServerList:MapName"] = "",
            ["Discord:Token"] = "true",
            ["Discord:Guild"] = "997787973775011850",
            ["Discord:StatusChannel:ChannelId"] = "1025774028255932497",
            ["Identity:Policies:Admin:RequireRoles:0"] = "Admin",
            //["Identity:Policies:Admin:RequireClaims:Test"] = "true",
            ["Gameplay:MoneyLimit"] = "1000000",
            ["Gameplay:MoneyPrecision"] = "4",
            ["Gameplay:DefaultInventorySize"] = "20",
            ["Gameplay:AfkCooldown"] = "20",
            ["Gameplay:CurrencyCulture"] = "pl-PL",
            ["Gameplay:Culture"] = "pl-PL",
            ["Database:ConnectionString"] = Environment.GetEnvironmentVariable("RealmCoreTestingDatabaseConnectionString"),
            ["Browser:BaseRemoteUrl"] = "https://localhost:7149",
            ["Assets:Base64Key"] = "ehFQcEzbNPfHt+CKpDJ41Q=="
        });

        hostBuilder.Services.AddRealmServerCore(hostBuilder.Configuration);
        outerApplicationBuilder?.Invoke(hostBuilder);

        hostBuilder.Services.AddSingleton<TestRealmResourceServer>();
        hostBuilder.Services.AddSingleton<IResourceServer>(x => x.GetRequiredService<TestRealmResourceServer>());
    }, serverBuilder =>
    {
        serverBuilder.AddDefaultLuaMappings();
        serverBuilder.AddResources();
        outerServerBuilder?.Invoke(serverBuilder);
    })
    {
        var waitHandle = new AutoResetEvent(false);
        var lifecycle = Host.Services.GetRequiredService<IHostApplicationLifetime>();
        lifecycle.ApplicationStarted.Register(() =>
        {
            waitHandle.Set();
        });

        lifecycle.ApplicationStopping.Register(() =>
        {
            ;
        });

        waitHandle.WaitOne(TimeSpan.FromSeconds(30.0));
    }

    public RealmTestingPlayer CreatePlayer()
    {
        var player = Server.AddFakePlayer();

        var testRealmResourceServer = Server.GetRequiredService<TestRealmResourceServer>();

        foreach (var resource in testRealmResourceServer.Resources)
        {
            player.TriggerResourceStarted(resource.NetId);
        }

        return player;
    }
}

public sealed class TestRealmResourceServer : IResourceServer
{
    public List<Resource> Resources = [];
    public void AddAdditionalResource(Resource resource, Dictionary<string, byte[]> files)
    {
        RealmResourceServer._resourceCounter++;
        Resources.Add(resource);
    }

    public void RemoveAdditionalResource(Resource resource)
    {
        RealmResourceServer._resourceCounter--;
    }

    public void Start() { }

    public void Stop() { }
}
