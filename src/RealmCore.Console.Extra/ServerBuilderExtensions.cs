﻿using Microsoft.Extensions.DependencyInjection;
using RealmCore.Configuration;
using RealmCore.Console.Integrations.Discord.Handlers;
using RealmCore.Console.Logic;
using RealmCore.Module.Discord;
using RealmCore.Module.Grpc;
using RealmCore.Module.Grpc.Options;
using RealmCore.Module.Web.AdminPanel;

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
            services.AddWebAdminPanelModule();

        });

        serverBuilder.AddLogic<GRpcLogic>();
        serverBuilder.AddLogic<DiscordIntegrationLogic>();
        serverBuilder.AddLogic<WebAdminPanelLogic>();
    }
}