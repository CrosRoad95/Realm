using SlipeServer.Resources.DGS;
using RealmCore.Resources.Addons.GuiSystem.DGS;
using RealmCore.Logging;
using Serilog.Events;
using Serilog;
using RealmCore.Sample.Logic;
using RealmCore.Sample.Utilities;
using RealmCore.Sample.Commands;
using RealmCore.Sample.Data.Validators;
using RealmCore.Server.Modules.Chat;
using RealmCore.Server.Modules.Integrations.External;
using RealmCore.Sample.Data;
using RealmCore.Sample.Server;
using RealmCore.Server.Extensions;

[assembly: ExcludeFromCodeCoverage]

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

        serverBuilder.AddLogic<PlayerJoinedLogic>();
        serverBuilder.AddLogic<SamplePickupsLogic>();
        serverBuilder.AddLogic<PlayerBindsLogic>();
        serverBuilder.AddLogic<ItemsLogic>();
        serverBuilder.AddLogic<VehicleUpgradesLogic>();
        serverBuilder.AddLogic<AchievementsLogic>();
        serverBuilder.AddLogic<WorldLogic>();
        serverBuilder.AddLogic<LevelsLogic>();
        serverBuilder.AddLogic<PlayerGameplayLogic>();
        serverBuilder.AddScopedLogic<CommandsLogic>();
        serverBuilder.AddLogic<MapsLogic>();
        serverBuilder.AddLogic<ExternalModulesLogic>();
        serverBuilder.AddLogic<ProceduralObjectsLogic>();
        serverBuilder.AddLogic<AssetsLogic>();
        serverBuilder.AddLogic<DefaultChatLogic>();
        serverBuilder.AddLogic<TestLogic>();

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

            services.AddSingleton<IUserDataSaver, TestSaver>();

            var realmLogger = new RealmLogger("RealmCore", LogEventLevel.Information);
            services.AddLogging(x => x.AddSerilog(realmLogger.GetLogger(), dispose: true));

            services.AddPlayerJoinedPipeline<PlayerBanPipeline>();
            services.AddPlayerJoinedPipeline<SamplePlayerJoinedPipeline>();
        });

        //serverBuilder.AddExtras(realmConfigurationProvider);
    })
    {
    }
}

public class SamplePlayerJoinedPipeline : IPlayerJoinedPipeline
{
    public async Task<bool> Next(Player player)
    {
        //throw new NotImplementedException();
        return true;
    }
}