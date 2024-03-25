using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace RealmCore.Configuration;

[ExcludeFromCodeCoverage]
public class RealmConfigurationProvider : IRealmConfigurationProvider
{
    public readonly IConfiguration _configuration;

    public RealmConfigurationProvider()
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

    public T? Get<T>(string name) => _configuration.GetSection(name).Get<T>();

    public T GetRequired<T>(string name) => _configuration.GetSection(name).Get<T>() ??
        throw new Exception($"Missing configuration '{name}'");

    public IConfigurationSection GetSection(string name) => _configuration.GetSection(name);
}