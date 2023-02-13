namespace Realm.Module.Grpc;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddGrpcModule(this IServiceCollection services)
    {
        services.AddSingleton<GreeterServiceStub>();
        services.AddSingleton<DiscordHandshakeServiceStub>();
        services.AddSingleton<DiscordStatusChannelServiceStub>();
        services.AddSingleton<DiscordConnectAccountChannelStub>();
        services.AddSingleton<IModule, GrpcModule>();
        return services;
    }
}
