namespace RealmCore.Configuration;

public class RealmConfigurationProvider : IRealmConfigurationProvider
{
    public readonly IConfiguration _configuration;

    public RealmConfigurationProvider()
    {
        _configuration =
            new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", false)
            .AddJsonFile("appsettings.development.json", true, true)
            .AddJsonFile("appsettings.local.json", true, true)
            .AddEnvironmentVariables()
            .Build();
    }

    public T? Get<T>(string name) => _configuration.GetSection(name).Get<T>();

    public T GetRequired<T>(string name) => _configuration.GetSection(name).Get<T>() ??
        throw new Exception($"Missing configuration '{name}'");

    public IConfigurationSection GetSection(string name) => _configuration.GetSection(name);
}