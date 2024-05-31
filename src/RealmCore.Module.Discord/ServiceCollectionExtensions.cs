namespace RealmCore.Module.Discord;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDiscordModule(this IServiceCollection services)
    {
        services.AddSingleton<DiscordHandshakeServiceStub>();
        services.AddSingleton<DiscordStatusChannelServiceStub>();
        services.AddSingleton<DiscordConnectUserChannelStub>();
        services.AddSingleton<DiscordPrivateMessagesChannelsStub>();
        services.AddSingleton<DiscordTextBasedCommandsStub>();
        services.AddSingleton(x => Handshake.BindService(x.GetRequiredService<DiscordHandshakeServiceStub>()));
        services.AddSingleton(x => StatusChannel.BindService(x.GetRequiredService<DiscordStatusChannelServiceStub>()));
        services.AddSingleton(x => ConnectUserChannel.BindService(x.GetRequiredService<DiscordConnectUserChannelStub>()));
        services.AddSingleton(x => PrivateMessagesChannels.BindService(x.GetRequiredService<DiscordPrivateMessagesChannelsStub>()));
        services.AddSingleton(x => Commands.BindService(x.GetRequiredService<DiscordTextBasedCommandsStub>()));

        services.AddSingleton<IDiscordService, DiscordService>();
        services.AddSingleton<IExternalModule, DiscordModule>();

        return services;
    }

    public static IServiceCollection AddDiscordStatusChannelUpdateHandler<T>(this IServiceCollection services) where T: class, IDiscordStatusChannelUpdateHandler
    {
        services.AddSingleton<IDiscordStatusChannelUpdateHandler, T>();
        services.AddSingleton<IDiscordHandler>(x => x.GetRequiredService<IDiscordStatusChannelUpdateHandler>());
        return services;
    }

    public static IServiceCollection AddDiscordConnectUserHandler<T>(this IServiceCollection services) where T: class, IDiscordConnectUserHandler
    {
        services.AddSingleton<IDiscordConnectUserHandler, T>();
        services.AddSingleton<IDiscordHandler>(x => x.GetRequiredService<IDiscordConnectUserHandler>());
        return services;
    }

    public static IServiceCollection AddDiscordPrivateMessageHandler<T>(this IServiceCollection services) where T: class, IDiscordPrivateMessageReceivedHandler
    {
        services.AddSingleton<IDiscordPrivateMessageReceivedHandler, T>();
        services.AddSingleton<IDiscordHandler>(x => x.GetRequiredService<IDiscordPrivateMessageReceivedHandler>());
        return services;
    }

    public static IServiceCollection AddDiscordTextBasedCommand<T>(this IServiceCollection services) where T: class, IDiscordTextBasedCommandHandler
    {
        services.AddSingleton<IDiscordTextBasedCommandHandler, T>();
        services.AddSingleton<IDiscordHandler>(x => x.GetRequiredService<IDiscordTextBasedCommandHandler>());
        return services;
    }

    public static IServiceCollection AddDiscordSupport(this IServiceCollection services)
    {
        services.AddSingleton<DiscordService>();
        return services;
    }
}
