using RealmCore.Console.Commands;
using RealmCore.Console.Utilities;
using RealmCore.Server.Logic;
using RealmCore.Server.Logic.Defaults;
using RealmCore.Console.Data.Validators;
using RealmCore.Resources.CEFBlazorGui;
using SlipeServer.Resources.DGS;
using RealmCore.Resources.GuiSystem;
using RealmCore.Resources.Addons.GuiSystem.DGS;
using RealmCore.Configuration;
using RealmCore.Logging;
using Serilog.Events;
using Serilog;

namespace RealmCore.Sample;

//using RealmCore.Console.Extra;

public class SampleServer
{
    public async Task Start()
    {
        bool withDgs = true;

        var realmConfigurationProvider = new RealmConfigurationProvider();
        var server = new RealmServer(realmConfigurationProvider, serverBuilder =>
        {
            if (withDgs)
            {
                serverBuilder.AddDGSResource(DGSVersion.Release_3_520);
                serverBuilder.AddGuiSystemResource(builder =>
                {
                    builder.AddGuiProvider(DGSGuiProvider.Name, DGSGuiProvider.LuaCode);
                    builder.SetGuiProvider(DGSGuiProvider.Name);
                }, new());
            }

            serverBuilder.AddCEFBlazorGuiResource();
            //serverBuilder.AddCEFBlazorGuiResource("../../../Server/BlazorGui/wwwroot", CEFGuiBlazorMode.Remote);

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
            serverBuilder.AddLogic<CommandsLogic>();
            serverBuilder.AddLogic<MapsLogic>();
            serverBuilder.AddLogic<DefaultModulesLogic>();
            serverBuilder.AddLogic<ProceduralObjectsLogic>();
            serverBuilder.AddLogic<AssetsLogic>();
            serverBuilder.AddLogic<DefaultBanLogic>();
            serverBuilder.AddLogic<DefaultChatLogic>();
            serverBuilder.AddLogic<BlazorGuiLogic>();

#if DEBUG
            if (withDgs)
            {
                serverBuilder.AddLogic<HotReloadLogic>("../../../Server/Gui");
            }
#endif

            serverBuilder.ConfigureServices(services =>
            {
                services.AddTransient<IValidator<LoginData>, LoginDataValidator>();

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
                    options.Mode = CEFGuiBlazorMode.Remote;
                    options.BrowserSize = new System.Drawing.Size(1024, 768);
                });

                var realmLogger = new RealmLogger("RealmCore", LogEventLevel.Information);
                services.AddLogging(x => x.AddSerilog(realmLogger.GetLogger(), dispose: true));
            });

            //serverBuilder.AddExtras(realmConfigurationProvider);
        });

        await server.Start();
        System.Console.WriteLine("Server stopped.");
    }
}