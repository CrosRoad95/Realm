namespace Realm.Server.Extensions;

public static class ServiceCollectionExtensions
{
    public static ServiceCollection AddCommand<TCommand>(this ServiceCollection services) where TCommand : class, ICommand
    {
        services.AddSingleton(new CommandTypeWrapper(typeof(TCommand)));
        services.AddTransient<TCommand>();
        return services;
    }

    public static ServiceCollection AddSeederProvider<TSeederProvider>(this ServiceCollection services) where TSeederProvider : class, ISeederProvider
    {
        services.AddSingleton<ISeederProvider, TSeederProvider>();
        return services;
    }
    
    public static ServiceCollection AddAsyncSeederProvider<TSeederProvider>(this ServiceCollection services) where TSeederProvider : class, IAsyncSeederProvider
    {
        services.AddSingleton<IAsyncSeederProvider, TSeederProvider>();
        return services;
    }


}
