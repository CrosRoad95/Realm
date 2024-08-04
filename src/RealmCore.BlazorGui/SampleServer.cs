using Serilog.Events;
using RealmCore.Server.Modules.Chat;
using RealmCore.BlazorGui.Data;
using RealmCore.BlazorGui.Commands;
using RealmCore.BlazorGui.Data.Validators;
using System.Diagnostics.CodeAnalysis;
using RealmCore.BlazorGui.Logic;
using Serilog;
using RealmCore.BlazorGui.Logging;
using RealmCore.Server.Modules.Players.Bans;
using RealmCore.BlazorGui.Modules.World;

[assembly: ExcludeFromCodeCoverage]

namespace RealmCore.BlazorGui;

public static class SampleServerExtensions
{
    public static IServiceCollection AddSampleServer(this IServiceCollection services)
    {
        services.AddTransient<IValidator<LoginData>, LoginDataValidator>();

        #region In game command
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

        services.AddHostedService<PlayerJoinedHostedLogic>();
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
        services.AddHostedService<LogClientDebugMessagesHostedService>();
        services.AddHostedService<WorldNodes>();
        return services;
    }
}

public class TestSaver : IUserDataSaver
{
    public Task SaveAsync(UserData userData, RealmPlayer player, CancellationToken cancellationToken = default)
    {
        player.Settings.Set(69, player.Name);
        return Task.CompletedTask;
    }
}
