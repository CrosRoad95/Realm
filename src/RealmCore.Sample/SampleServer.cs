using RealmCore.Logging;
using Serilog.Events;
using Serilog;
using RealmCore.Sample.Commands;
using RealmCore.Sample.Data.Validators;
using RealmCore.Sample.Data;
using RealmCore.Sample.Server;
using RealmCore.Module.Discord;
using RealmCore.Server.Modules.Chat;

[assembly: ExcludeFromCodeCoverage]

namespace RealmCore.Sample;

public static class SampleServerExtensions
{
    public static IServiceCollection AddSampleServer(this IServiceCollection services)
    {
        services.AddTransient<IValidator<LoginData>, LoginDataValidator>();

        #region In game command
        services.AddInGameCommand<CreateGroupCommand>();
        services.AddInGameCommand<GiveItemCommand>();
        services.AddInGameCommand<GiveLicenseCommand>();
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

        services.AddDiscordSupport();
        services.AddGuiSystemServices();

        services.WithGuiSystem();

        services.AddHostedService<PlayerJoinedHostedLogic>();
        services.AddHostedService<SamplePickupsHostedService>();
        services.AddHostedService<PlayerBindsHostedService>();
        services.AddHostedService<ItemsHostedService>();
        services.AddHostedService<VehicleUpgradesHostedService>();
        services.AddHostedService<AchievementsHostedService>();
        services.AddHostedService<WorldHostedService>();
        services.AddHostedService<LevelsHostedService>();
        services.AddHostedService<PlayerGameplayHostedService>();
        services.AddHostedService<CommandsHostedService>();
        services.AddHostedService<MapsHostedService>();
        services.AddHostedService<ProceduralObjectsHostedService>();
        services.AddHostedService<AssetsManager>();
        services.AddHostedService<DefaultChatHostedService>();
        services.AddHostedService<TestHostedService>();
        services.AddHostedService<AntiCheatHostedService>();
        return services;
    }
}
