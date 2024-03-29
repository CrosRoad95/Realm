﻿using Microsoft.Extensions.Primitives;

namespace RealmCore.TestingTools;

public class TestConfigurationProvider : IConfiguration
{
    private readonly IConfiguration _configuration;
    public TestConfigurationProvider(string connectionString, int? basePort = null)
    {
        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Server:IsVoiceEnabled"] = "true",
                ["Server:ServerName"] = "Default New-RealmCore Test server",
                ["Server:Port"] = (basePort ?? 20000).ToString(),
                ["Server:HttpPort"] = ((basePort ?? 20000) + 1).ToString(),
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
                ["Database:ConnectionString"] = connectionString
            }).Build();
    }

    public string? this[string key] { get => _configuration[key]; set => _configuration[key] = value; }

    public IEnumerable<IConfigurationSection> GetChildren() => _configuration.GetChildren();

    public IChangeToken GetReloadToken() => _configuration.GetReloadToken();

    public IConfigurationSection GetSection(string key) => _configuration.GetSection(key);
}
