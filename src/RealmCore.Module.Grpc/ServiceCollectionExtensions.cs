using Discord;
using Grpc.Core;
using Grpc.Core.Logging;
using Grpc.Net.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RealmCore.Module.Grpc.Options;

namespace RealmCore.Module.Grpc;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRealmGrpc(this IServiceCollection services)
    {
        services.AddSingleton<GreeterServiceStub>();
        services.AddSingleton(x => Greeter.BindService(x.GetRequiredService<GreeterServiceStub>()));

        services.AddSingleton(x =>
        {
            var options = x.GetRequiredService<IOptions<GrpcOptions>>();
            var logger = x.GetRequiredService<ILogger<GrpcChannel>>();
            var address = $"http://{options.Value.RemoteHost}:{options.Value.RemotePort}";
            var channel = GrpcChannel.ForAddress(address);
            logger.LogInformation("Created Grpc channel for address: {address}", address);
            return channel;
        });

        return services;
    }
}
