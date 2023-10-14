using RealmCore.Server.Logic;
using RealmCore.Server.Logic.Defaults;
using RealmCore.Resources.Browser;
using SlipeServer.Resources.DGS;
using RealmCore.Resources.Addons.GuiSystem.DGS;
using RealmCore.Configuration;
using RealmCore.Logging;
using Serilog.Events;
using Serilog;
using RealmCore.Sample.Logic;
using RealmCore.Sample.Utilities;
using RealmCore.Sample.Commands;
using RealmCore.Sample.Data.Validators;
using RealmCore.Sample.Data;

namespace RealmCore.Sample;

//using RealmCore.Console.Extra;

public class SampleServer : RealmServer
{
    static bool withDgs = true;

    public SampleServer() : base(new RealmConfigurationProvider(), serverBuilder =>
    {
        if (withDgs)
        {
            serverBuilder.WithGuiSystem();
            serverBuilder.AddDGSResource(DGSVersion.Release_3_520);
            serverBuilder.AddGuiSystemResource(builder =>
            {
                builder.AddGuiProvider(DGSGuiProvider.Name, DGSGuiProvider.LuaCode);
                builder.SetGuiProvider(DGSGuiProvider.Name);
            }, new());
        }

        serverBuilder.AddBrowserResource();
        //serverBuilder.AddBrowserResource("../../../Server/BlazorGui/wwwroot", BrowserMode.Remote);

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
        serverBuilder.AddLogic<PlayTimeComponentLogic>();

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

            var realmLogger = new RealmLogger("RealmCore", LogEventLevel.Information);
            services.AddLogging(x => x.AddSerilog(realmLogger.GetLogger(), dispose: true));
        });

        //serverBuilder.AddExtras(realmConfigurationProvider);
    })
    {
    }
}