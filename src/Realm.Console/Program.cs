
using Realm.Console.Discord.Handler;
using Realm.Console.Services;
using Realm.Module.Discord.Interfaces;
using Realm.Resources.Assets.Interfaces;

Directory.SetCurrentDirectory(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly()!.Location)!);

var builder = new RPGServerBuilder();
builder.AddDefaultLogger()
    .AddDefaultConsole()
    .AddDefaultConfiguration();

SemaphoreSlim semaphore = new(0);

var server = builder.Build(null, extraBuilderSteps: serverBuilder =>
{
    serverBuilder.AddLogic<PlayerJoinedLogic>();
    serverBuilder.AddLogic<CommandsLogic>();
    serverBuilder.AddLogic<SamplePickupsLogic>();
    serverBuilder.AddLogic<PlayerBindsLogic>();
    serverBuilder.AddLogic<ItemsLogic>();
    serverBuilder.AddLogic<VehicleUpgradesLogic>();
    serverBuilder.AddLogic<AchievementsLogic>();
    serverBuilder.AddLogic<WorldLogic>();
    serverBuilder.AddLogic<LevelsLogic>();
    serverBuilder.AddLogic<PlayerGameplayLogic>();
    serverBuilder.AddLogic<DiscordIntegrationLogic>();
#if DEBUG
    serverBuilder.AddLogic<HotReloadLogic>("../../../Server/Gui");
#endif
    serverBuilder.ConfigureServices(x =>
    {
        x.AddTransient<IServerAssetsProvider, ServerAssetsService>();

        #region Discord integration specific
        x.AddSingleton<IDiscordStatusChannelUpdateHandler, DiscordStatusChannelUpdateHandler>();
        #endregion
    });
});

Console.CancelKeyPress += (sender, args) =>
{
    try
    {
        server.Stop().Wait();
    }
    finally
    {
        semaphore.Release();
    }
};

await server.Start();
server.GetRequiredService<IConsole>().Start();
await semaphore.WaitAsync();
Console.WriteLine("Server stopped.");