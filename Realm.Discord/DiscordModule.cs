namespace Realm.Discord;

public class DiscordModule : IModule
{
    public string Name => "Discord";

    public void Configure(IServiceCollection services)
    {
        services.AddSingleton(x => x.GetRequiredService<ConfigurationProvider>().Get<DiscordConfiguration>("discord"));
        services.AddSingleton<DiscordIntegration>();
        services.AddSingleton<IDiscord>(x => x.GetRequiredService<DiscordIntegration>());
        services.AddSingleton<StatusChannel>();
        services.AddSingleton<ServerConnectionChannel>();
        services.AddSingleton(new DiscordSocketClient(new DiscordSocketConfig { GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.GuildMembers }));
        services.AddSingleton<CommandHandler>();
        services.AddSingleton<IBotdIdProvider, BotIdProvider>();
    }

    public void Init(IServiceProvider serviceProvider)
    {
        var discordIntegration = serviceProvider.GetRequiredService<DiscordIntegration>();
        var _ = Task.Run(discordIntegration.StartAsync);
    }

    public T GetInterface<T>() where T : class => throw new NotSupportedException();

    public void PostInit(IServiceProvider serviceProvider)
    {
        var discordIntegration = serviceProvider.GetRequiredService<DiscordIntegration>();

        var scriptingModule = serviceProvider.GetRequiredService<IEnumerable<IModule>>().FirstOrDefault(x => x.Name == "Scripting");
        if (scriptingModule != null)
            discordIntegration.InitializeScripting(scriptingModule.GetInterface<IScriptingModuleInterface>());
    }

    public void Reload()
    {

    }

    public int GetPriority() => 100;
}
