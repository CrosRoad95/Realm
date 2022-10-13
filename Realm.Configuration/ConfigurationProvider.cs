namespace Realm.Configuration;

public class ConfigurationProvider
{
    public readonly IConfiguration Configuration;
    public ConfigurationProvider(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public ConfigurationProvider()
    {
        Configuration = AddRealmConfiguration(new ConfigurationBuilder()).Build();
    }

    public T Get<T>(string name) => Configuration.GetSection(name).Get<T>();

    public static IConfigurationBuilder AddRealmConfiguration(IConfigurationBuilder configurationBuilder, string? basePath = null)
    {
        var previousDirectory = Directory.GetCurrentDirectory();
        if(basePath != null)
            Directory.SetCurrentDirectory(basePath);

        configurationBuilder = configurationBuilder
            .AddJsonFile(Path.Join(basePath, "appsettings.server.json"), false)
            .AddJsonFile(Path.Join(basePath, "appsettings.server.development.json"), true, true)
            .AddJsonFile(Path.Join(basePath, "appsettings.server.local.json"), true, true)
            .AddEnvironmentVariables();
        Directory.SetCurrentDirectory(previousDirectory);
        return configurationBuilder;
    }
}