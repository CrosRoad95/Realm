namespace Realm.Configuration;

public class ConfigurationProvider
{
    public readonly IConfiguration Configuration;
    public ConfigurationProvider()
    {
        Configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", false)
            .AddJsonFile("appsettings.development.json", true, true)
            .AddJsonFile("appsettings.local.json", true, true)
            .AddEnvironmentVariables()
            .Build();
    }

    public T Get<T>(string name) => Configuration.GetSection(name).Get<T>();
}