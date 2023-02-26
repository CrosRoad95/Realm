using Realm.Interfaces.Extend;
using Realm.Module.Discord.Services;
using Realm.Module.Discord.Stubs;

namespace Realm.Module.Discord;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDiscordModule(this IServiceCollection services)
    {
        services.AddSingleton<DiscordHandshakeServiceStub>();
        services.AddSingleton<DiscordStatusChannelServiceStub>();
        services.AddSingleton<DiscordConnectAccountChannelStub>();

        services.AddSingleton<IDiscordService, DiscordService>();
        services.AddSingleton<IModule, DiscordModule>();
        return services;
    }
}
