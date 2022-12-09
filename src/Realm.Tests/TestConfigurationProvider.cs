using Realm.Configuration;

namespace Realm.Tests;

internal class TestConfigurationProvider : RealmConfigurationProvider
{
    public IConfiguration Configuration { get; private set; }
    public TestConfigurationProvider()
    {
        Configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                ["server:isVoiceEnabled"] = "true",
                ["server:serverName"] = "Default New-Realm Test server",
                ["server:port"] = "22003",
                ["server:httpPort"] = "2205",
                ["server:maxPlayerCount"] = "128",
                ["serverlist:gameType"] = "",
                ["serverlist:mapName"] = "",
                ["scripting:enabled"] = "true",
                ["discord:token"] = "true",
                ["discord:guild"] = "997787973775011850",
                ["discord:statusChannel:channelId"] = "1025774028255932497",
                ["Identity:Policies:Admin:RequireRoles:0"] = "Admin",
                ["Identity:Policies:Admin:RequireClaims:Test"] = "true",
            }).Build();
    }
}
