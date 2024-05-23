using Microsoft.Extensions.Configuration;
using RealmCore.Console.Extra.Integrations.Discord.Handlers;
using RealmCore.Module.Discord;
using RealmCore.Module.Grpc;
using RealmCore.Module.Grpc.Options;

namespace RealmCore.Console.Extra;

public static class ServerBuilderExtensions
{
    public static void AddExtras(this SlipeServer.Server.ServerBuilders.ServerBuilder serverBuilder, IConfiguration configuration)
    {
        serverBuilder.ConfigureServices(services =>
        {
            services.Configure<GrpcOptions>(configuration.GetSection("Grpc"));

            #region Discord integration specific
            services.AddSingleton<IDiscordStatusChannelUpdateHandler, DiscordStatusChannelUpdateHandler>();
            services.AddSingleton<IDiscordConnectUserHandler, DiscordConnectUserHandler>();
            services.AddSingleton<IDiscordPrivateMessageReceivedHandler, DiscordPrivateMessageReceivedHandler>();
            services.AddSingleton<IDiscordTextBasedCommandHandler, TextBasedCommandHandler>();
            #endregion

            services.AddRealmGrpc();
            services.AddDiscordModule();

        });
    }
}
