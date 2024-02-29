using RealmCore.Interfaces.Extend;
using RealmCore.Module.Discord.Stubs;

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
}
