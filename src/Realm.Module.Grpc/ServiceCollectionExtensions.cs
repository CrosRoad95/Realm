using Grpc.Net.Client;

namespace Realm.Module.Grpc;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddGrpcModule(this IServiceCollection services)
    {
        services.AddSingleton<GreeterServiceStub>();
        services.AddSingleton(GrpcChannel.ForAddress("http://localhost:22020"));
        return services;
    }
}
