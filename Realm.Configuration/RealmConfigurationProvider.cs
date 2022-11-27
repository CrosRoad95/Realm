namespace Realm.Configuration;

public class RealmConfigurationProvider
{
    public readonly IConfiguration Configuration;
    public RealmConfigurationProvider(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public RealmConfigurationProvider()
    {
        Configuration = AddRealmConfiguration(new ConfigurationBuilder()).Build();
    }

    public T? Get<T>(string name) => Configuration.GetSection(name).Get<T>();
    public T GetRequired<T>(string name) => Configuration.GetSection(name).Get<T>() ??
        throw new Exception($"Missing configuration '{name}'");

    public static IConfigurationBuilder AddRealmConfiguration(IConfigurationBuilder configurationBuilder, string? basePath = null)
    {
        var previousDirectory = Directory.GetCurrentDirectory();
        if(basePath != null)
            Directory.SetCurrentDirectory(basePath);

        configurationBuilder = configurationBuilder
            .AddJsonFile(Path.Join(basePath, "appsettingsServer.json"), false)
            .AddJsonFile(Path.Join(basePath, "appsettingsServer.development.json"), true, true)
            .AddJsonFile(Path.Join(basePath, "appsettingsServer.local.json"), true, true)
            .AddEnvironmentVariables();
        Directory.SetCurrentDirectory(previousDirectory);
        return configurationBuilder;
    }
}