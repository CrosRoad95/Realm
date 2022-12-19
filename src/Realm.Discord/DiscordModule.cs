namespace Realm.Module.Discord;

public class DiscordModule : IModule
{
    public string Name => "Discord";

    public void Configure(IServiceCollection services)
    {
        services.AddSingleton<DiscordIntegration>();

        services.AddSingleton<DiscordVerificationHandler>();
        services.AddSingleton<DiscordUserChangedHandler>();
    }

    public void Init(IServiceProvider serviceProvider)
    {
        var discordIntegration = serviceProvider.GetRequiredService<DiscordIntegration>();
    }

    public T GetInterface<T>() where T : class => throw new NotSupportedException();
}
