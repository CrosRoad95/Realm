namespace Realm.Server.Extensions;

public static class ServiceCollectionExtensions
{
    public static ServiceCollection AddCommand<TCommand>(this ServiceCollection services) where TCommand : class, ICommand
    {
        services.AddSingleton(new CommandTypeWrapper(typeof(TCommand)));
        services.AddTransient<TCommand>();
        return services;
    }
}
