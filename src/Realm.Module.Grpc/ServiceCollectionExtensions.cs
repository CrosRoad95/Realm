namespace Realm.Module.Grpc;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddGrpcModule(this IServiceCollection services)
    {
        services.AddSingleton<GreeterServiceStub>();
        return services;
    }
}
