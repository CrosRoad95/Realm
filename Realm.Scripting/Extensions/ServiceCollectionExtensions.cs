﻿namespace Realm.Scripting.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddScripting(this IServiceCollection services)
    {
        services.AddSingleton<IWorld, World>();
        services.AddSingleton<IEvent, Event>();
        services.AddSingleton<IReloadable>(x => x.GetRequiredService<IEvent>());
        services.AddSingleton<IScripting, Javascript>();
        return services;
    }
}