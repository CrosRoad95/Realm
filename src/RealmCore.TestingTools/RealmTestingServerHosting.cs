using Microsoft.Extensions.Hosting;
using RealmCore.WebHosting;

namespace RealmCore.TestingTools;

public class RealmTestingServerHosting : TestingServerHosting<TestingPlayer>
{
    public RealmTestingServerHosting(Action<HostApplicationBuilder>? outerApplicationBuilder = null, Action<ServerBuilder>? outerServerBuilder = null) : base(applicationBuilder =>
    {
        applicationBuilder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
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
            ["Database:ConnectionString"] = Environment.GetEnvironmentVariable("RealmCoreTestingDatabaseConnectionString")
        });
        applicationBuilder.AddRealmServer<RealmTestingPlayer>();
        outerApplicationBuilder?.Invoke(applicationBuilder);
    }, serverBuilder =>
    {
        outerServerBuilder?.Invoke(serverBuilder);
    })
    {

    }
}
