using RealmCore.Interfaces.Extend;
using RealmCore.Module.Discord.Services;
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

        services.AddSingleton<IDiscordService, DiscordService>();
        services.AddSingleton<IModule, DiscordModule>();
        return services;
    }
}
