using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.Extensions.Options;
using RealmCore.Module.Grpc.Options;

namespace RealmCore.Module.Grpc;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddGrpcModule(this IServiceCollection services)
    {
        services.AddSingleton(x =>
        {
            var options = x.GetRequiredService<IOptions<GrpcOptions>>();
            var serverServiceDefinitions = x.GetRequiredService<IEnumerable<ServerServiceDefinition>>();

            var server = new Server
            {
                Ports =
                {
                    new ServerPort(options.Value.Host, options.Value.Port, ServerCredentials.Insecure)
                },
            };
            foreach (var item in serverServiceDefinitions)
                server.Services.Add(item);

            return server;
        });
        services.AddSingleton<GreeterServiceStub>();

        services.AddSingleton(x =>
        {
            var options = x.GetRequiredService<IOptions<GrpcOptions>>();
            return GrpcChannel.ForAddress($"http://{options.Value.RemoteHost}:{options.Value.RemotePort}");
        });

        services.AddHostedService<GrpcServerHostedService>();
        return services;
    }
}
