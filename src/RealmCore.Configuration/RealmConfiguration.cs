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
        var configurationBuilder =
            new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", false)
            .AddJsonFile("appsettings.server.json", true, true)
            .AddJsonFile("appsettings.development.json", true, true)
            .AddJsonFile("appsettings.local.json", true, true)
            .AddEnvironmentVariables();

        var entryAssembly = Assembly.GetEntryAssembly();
        if (entryAssembly != null)
            configurationBuilder.AddUserSecrets(entryAssembly);

        _configuration = configurationBuilder.Build();
    }

    public string? this[string key] { get => _configuration[key]; set => _configuration[key] = value; }

    public IEnumerable<IConfigurationSection> GetChildren() => _configuration.GetChildren();

    public IChangeToken GetReloadToken() => _configuration.GetReloadToken();

    public IConfigurationSection GetSection(string key) => _configuration.GetSection(key);
}
