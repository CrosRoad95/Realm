namespace Realm.Discord;

public class DiscordModule : IModule
{
    public string Name => "Discord";

    public void Configure(IServiceCollection services)
    {
        services.AddSingleton(x => x.GetRequiredService<ConfigurationProvider>().Get<DiscordConfiguration>("discord"));
        services.AddSingleton<DiscordIntegration>();
        services.AddSingleton<IDiscord>(x => x.GetRequiredService<DiscordIntegration>());
        services.AddSingleton<IStatusChannel, StatusChannel>();
        services.AddSingleton<IBotdIdProvider, BotIdProvider>();
    }

    public void Init(IServiceProvider serviceProvider)
    {
        var discordIntegration = serviceProvider.GetRequiredService<DiscordIntegration>();
        var _ = Task.Run(discordIntegration.StartAsync);
    }
}
