using RealmCore.Server.Extensions;
using RealmCore.Console.Commands;
using RealmCore.Console.Utilities;
using RealmCore.Module.Discord.Interfaces;
using RealmCore.Server.Integrations.Discord.Handlers;
using RealmCore.Server.Logic;
using RealmCore.Server.Logic.Defaults;
using RealmCore.Console.Data.Validators;

Directory.SetCurrentDirectory(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly()!.Location)!);

var builder = new RPGServerBuilder();
builder.AddDefaultLogger()
    .AddDefaultConsole()
    .AddDefaultConfiguration();

SemaphoreSlim semaphore = new(0);

var server = builder.Build(null, extraBuilderSteps: serverBuilder =>
{
    serverBuilder.AddLogic<DefaultCommandsLogic>();

    serverBuilder.AddLogic<PlayerJoinedLogic>();
    serverBuilder.AddLogic<SamplePickupsLogic>();
    serverBuilder.AddLogic<PlayerBindsLogic>();
    serverBuilder.AddLogic<ItemsLogic>();
    serverBuilder.AddLogic<VehicleUpgradesLogic>();
    serverBuilder.AddLogic<AchievementsLogic>();
    serverBuilder.AddLogic<WorldLogic>();
    serverBuilder.AddLogic<LevelsLogic>();
    serverBuilder.AddLogic<PlayerGameplayLogic>();
    serverBuilder.AddLogic<DiscordIntegrationLogic>();
    serverBuilder.AddLogic<CommandsLogic>();
    serverBuilder.AddLogic<MapsLogic>();
    serverBuilder.AddLogic<DefaultModulesLogic>();
    serverBuilder.AddLogic<ProceduralObjectsLogic>();
    serverBuilder.AddLogic<AssetsLogic>();
    serverBuilder.AddLogic<DefaultBanLogic>();
#if DEBUG
    serverBuilder.AddLogic<HotReloadLogic>("../../../Server/Gui");
#endif
    serverBuilder.ConfigureServices(services =>
    {
        services.AddTransient<IValidator<LoginData>, LoginDataValidator>();

        #region Discord integration specific
        services.AddSingleton<IDiscordStatusChannelUpdateHandler, DefaultDiscordStatusChannelUpdateHandler>();
        services.AddSingleton<IDiscordConnectUserHandler, DefaultDiscordConnectUserHandler>();
        services.AddSingleton<IDiscordPrivateMessageReceived, DefaultDiscordPrivateMessageReceivedHandler>();
        services.AddSingleton<IDiscordTextBasedCommandHandler, DefaultTextBasedCommandHandler>();
        #endregion

        #region In game command
        services.AddInGameCommand<CreateGroupCommand>();
        services.AddInGameCommand<GiveItemCommand>();
        services.AddInGameCommand<GiveLicenseCommand>();
        services.AddInGameCommand<GPCommand>();
        services.AddInGameCommand<InventoryCommand>();
        services.AddInGameCommand<LicensesCommand>();
        services.AddInGameCommand<TakeItemCommand>();
        services.AddInGameCommand<AddPointsCommand>();
        services.AddInGameCommand<JobsStatsCommand>();
        services.AddInGameCommand<JobsStatsAllCommand>();
        services.AddInGameCommand<GiveRewardCommand>();
        services.AddInGameCommand<Display3dRing>();
        services.AddInGameCommand<CurrencyCommand>();
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