using RealmCore.Configuration;
using RealmCore.Discord.Integration.Stubs;
using RealmCore.Module.Grpc.Options;

namespace RealmCore.Discord.Integration.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRealmDiscordBotIntegration(this IServiceCollection services, RealmConfigurationProvider realmConfigurationProvider)
    {
        services.Configure<GrpcOptions>(realmConfigurationProvider.GetSection("Grpc"));
        services.Configure<DiscordBotOptions>(realmConfigurationProvider.GetSection("Discord"));
        services.AddSingleton<IRealmDiscordClient, RealmDiscordClient>();
        services.AddSingleton(new DiscordSocketClient(new DiscordSocketConfig
        {
            GatewayIntents = GatewayIntents.All
        }));
        services.AddSingleton<CommandHandler>();
        services.AddSingleton<BotIdProvider>();

        services.AddSingleton<GrpcServer>();
        services.AddSingleton<MessagingServiceStub>();
        services.AddSingleton<TextBasedCommands>();

        services.AddSingleton(x =>
        {
            var options = x.GetRequiredService<IOptions<GrpcOptions>>();
            return GrpcChannel.ForAddress($"http://{options.Value.Host}:{options.Value.Port}");
        });

        return services;
    }

    public static IServiceCollection AddDiscordChannel<T>(this IServiceCollection services)
        where T : ChannelBase
    {
        services.AddSingleton<ChannelBase, T>();
        return services;
    }
}
