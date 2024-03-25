namespace RealmCore.Configuration;

public static class ConfigurationExtensions
{
    public static T? Get<T>(this IConfiguration configuration, string name) => configuration.GetSection(name).Get<T>();

    public static T GetRequired<T>(this IConfiguration configuration, string name) => configuration.GetSection(name).Get<T>() ??
        throw new Exception($"Missing configuration '{name}'");

    public static IConfigurationSection GetSection(this IConfiguration configuration, string name) => configuration.GetSection(name);
}