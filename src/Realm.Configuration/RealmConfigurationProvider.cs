﻿namespace Realm.Configuration;

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

    public static IConfigurationBuilder AddRealmConfiguration(IConfigurationBuilder configurationBuilder)
    {
        configurationBuilder = configurationBuilder
            .AddJsonFile("appsettingsServer.json", false)
            .AddJsonFile("appsettingsServer.development.json", true, true)
            .AddJsonFile("appsettingsServer.local.json", true, true)
            .AddEnvironmentVariables();

        return configurationBuilder;
    }
}