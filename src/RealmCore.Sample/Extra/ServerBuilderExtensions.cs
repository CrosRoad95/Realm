using Microsoft.Extensions.Configuration;
using RealmCore.Module.Discord;
using RealmCore.Module.Grpc;
using RealmCore.Module.Grpc.Options;
using RealmCore.Sample.Extra.Integrations.Discord.Handlers;

namespace RealmCore.Sample.Extra;

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
