namespace RealmCore.Tests;

public class TestConfigurationProvider : IRealmConfigurationProvider
{
    private readonly IConfiguration _configuration;
    public TestConfigurationProvider(string connectionString, int? basePort = null)
    {
        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["server:isVoiceEnabled"] = "true",
                ["server:serverName"] = "Default New-RealmCore Test server",
                ["server:port"] = (basePort ?? 20000).ToString(),
                ["server:httpPort"] = ((basePort ?? 20000) + 1).ToString(),
                ["server:maxPlayerCount"] = "128",
                ["serverlist:gameType"] = "",
                ["serverlist:mapName"] = "",
                ["discord:token"] = "true",
                ["discord:guild"] = "997787973775011850",
                ["discord:statusChannel:channelId"] = "1025774028255932497",
                ["Identity:Policies:Admin:RequireRoles:0"] = "Admin",
                //["Identity:Policies:Admin:RequireClaims:Test"] = "true",
                ["Gameplay:MoneyLimit"] = "1000000",
                ["Gameplay:MoneyPrecision"] = "4",
                ["Gameplay:DefaultInventorySize"] = "20",
                ["Gameplay:AfkCooldown"] = "20",
                ["Database:ConnectionString"] = connectionString
            }).Build();
    }

    public T? Get<T>(string name) => _configuration.GetSection(name).Get<T>();
    public T GetRequired<T>(string name) => _configuration.GetSection(name).Get<T>() ??
        throw new Exception($"Missing configuration '{name}'");

    public IConfigurationSection GetSection(string name) => _configuration.GetSection(name);
}
