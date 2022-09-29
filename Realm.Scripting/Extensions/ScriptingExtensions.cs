namespace Realm.Scripting.Extensions;

public static class PersistanceExtensions
{
    public static IServiceCollection AddScripting(this IServiceCollection services)
    {
        services.AddSingleton<IScripting, Javascript>();
        return services;
    }
}
