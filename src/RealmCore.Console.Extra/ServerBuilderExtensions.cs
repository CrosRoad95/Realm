using Microsoft.Extensions.DependencyInjection;
using RealmCore.Configuration;
using RealmCore.Console.Extra.Integrations.Discord.Handlers;
using RealmCore.Module.Discord;
using RealmCore.Module.Grpc;
using RealmCore.Module.Grpc.Options;

namespace RealmCore.Console.Extra;

public static class ServerBuilderExtensions
{
    public static void AddExtras(this SlipeServer.Server.ServerBuilders.ServerBuilder serverBuilder, RealmConfigurationProvider realmConfigurationProvider)
    {
        serverBuilder.ConfigureServices(services =>
        {
            services.Configure<GrpcOptions>(realmConfigurationProvider.GetSection("Grpc"));

            #region Discord integration specific
            services.AddSingleton<IDiscordStatusChannelUpdateHandler, DiscordStatusChannelUpdateHandler>();
            services.AddSingleton<IDiscordConnectUserHandler, DiscordConnectUserHandler>();
            services.AddSingleton<IDiscordPrivateMessageReceived, DiscordPrivateMessageReceivedHandler>();
            services.AddSingleton<IDiscordTextBasedCommandHandler, TextBasedCommandHandler>();
            #endregion

            services.AddGrpcModule();
            services.AddDiscordModule();

        });

        serverBuilder.AddLogic<GRpcLogic>();
        serverBuilder.AddLogic<DiscordIntegrationLogic>();
    }
}
