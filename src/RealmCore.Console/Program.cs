using RealmCore.Console.Commands;
using RealmCore.Console.Utilities;
using RealmCore.Module.Discord.Interfaces;
using RealmCore.Server.Integrations.Discord.Handlers;
using RealmCore.Server.Logic;
using RealmCore.Server.Logic.Defaults;
using RealmCore.Console.Data.Validators;
using RealmCore.Resources.CEFBlazorGui;
using SlipeServer.Resources.DGS;
using RealmCore.Resources.GuiSystem;
using RealmCore.Resources.Addons.GuiSystem.DGS;
using RealmCore.Module.Web.AdminPanel;
using RealmCore.Module.Grpc;

Directory.SetCurrentDirectory(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly()!.Location)!);

var builder = new RPGServerBuilder();
builder.AddDefaultLogger()
    .AddDefaultConsole()
    .AddDefaultConfiguration();

bool withDgs = true;
SemaphoreSlim semaphore = new(0);

var server = builder.Build(null, extraBuilderSteps: serverBuilder =>
{
    if(withDgs)
    {
        serverBuilder.AddDGSResource(DGSVersion.Release_3_520);
        serverBuilder.AddGuiSystemResource(builder =>
        {
            builder.AddGuiProvider(DGSGuiProvider.Name, DGSGuiProvider.LuaCode);
            builder.SetGuiProvider(DGSGuiProvider.Name);
        }, new());
    }

    serverBuilder.AddCEFBlazorGuiResource("../../../Server/BlazorGui/wwwroot", CEFGuiBlazorMode.Dev);

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
    serverBuilder.AddLogic<DefaultChatLogic>();
    serverBuilder.AddLogic<BlazorGuiLogic>();
    serverBuilder.AddLogic<GRpcLogic>();
    serverBuilder.AddLogic<WebAdminPanelLogic>();

#if DEBUG
    if (withDgs)
    {
        serverBuilder.AddLogic<HotReloadLogic>("../../../Server/Gui");
    }
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

        #region Web admin panel
        services.AddWebAdminPanelModule();
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

        services.Configure<BlazorOptions>(options =>
        {
            options.Mode = CEFGuiBlazorMode.Dev;
            options.BrowserSize = new System.Drawing.Size(1024, 768);
        });
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