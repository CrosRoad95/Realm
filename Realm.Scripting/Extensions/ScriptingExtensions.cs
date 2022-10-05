using Realm.Interfaces.Scripting.Classes;
using Realm.Scripting.Classes;

namespace Realm.Scripting.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddScripting(this IServiceCollection services)
    {
        services.AddSingleton<IWorld, World>();
        services.AddSingleton<IScripting, Javascript>();
        return services;
    }
}
