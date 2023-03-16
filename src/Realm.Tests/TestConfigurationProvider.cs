namespace Realm.Tests;

internal class TestConfigurationProvider : IRealmConfigurationProvider
{
    private readonly IConfiguration _configuration;
    public TestConfigurationProvider()
    {
        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                ["server:isVoiceEnabled"] = "true",
                ["server:serverName"] = "Default New-Realm Test server",
                ["server:port"] = "22003",
                ["server:httpPort"] = "2205",
                ["server:maxPlayerCount"] = "128",
                ["serverlist:gameType"] = "",
                ["serverlist:mapName"] = "",
                ["discord:token"] = "true",
                ["discord:guild"] = "997787973775011850",
                ["discord:statusChannel:channelId"] = "1025774028255932497",
                ["Identity:Policies:Admin:RequireRoles:0"] = "Admin",
                ["Identity:Policies:Admin:RequireClaims:Test"] = "true",
                ["Gameplay:MoneyLimit"] = "10000",
                ["Gameplay:MoneyPrecision"] = "4",
                ["Gameplay:DefaultInventorySize"] = "20",
                ["Database:Provider"] = "InMemory",
            }).Build();
    }

    public T? Get<T>(string name) => _configuration.GetSection(name).Get<T>();
    public T GetRequired<T>(string name) => _configuration.GetSection(name).Get<T>() ??
        throw new Exception($"Missing configuration '{name}'");

    public IConfigurationSection GetSection(string name) => _configuration.GetSection(name);
}
