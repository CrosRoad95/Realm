using Microsoft.Extensions.Primitives;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace RealmCore.Configuration;

[ExcludeFromCodeCoverage]
public class RealmConfiguration : IConfiguration
{
    public readonly IConfiguration _configuration;

    public RealmConfiguration()
    {
        var configurationBuilder = new ConfigurationBuilder();

        var entryAssembly = Assembly.GetEntryAssembly();
        if (entryAssembly != null)
            configurationBuilder.AddUserSecrets(entryAssembly);

        configurationBuilder.AddJsonFile("appsettings.json", false);
        configurationBuilder.AddJsonFile("appsettings.server.json", true, true);
        configurationBuilder.AddJsonFile("appsettings.development.json", true, true);
        configurationBuilder.AddJsonFile("appsettings.local.json", true, true);
        configurationBuilder.AddEnvironmentVariables();

        _configuration = configurationBuilder.Build();
    }

    public string? this[string key] { get => _configuration[key]; set => _configuration[key] = value; }

    public IEnumerable<IConfigurationSection> GetChildren() => _configuration.GetChildren();

    public IChangeToken GetReloadToken() => _configuration.GetReloadToken();

    public IConfigurationSection GetSection(string key) => _configuration.GetSection(key);
}
