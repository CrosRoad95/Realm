using SlipeServer.Resources.Scoreboard;

namespace RealmCore.Server.Extensions.Resources;

public static class ScoreboardServiceExtensions
{
    public static void SetEnabledTo(this ScoreboardService scoreboardService, Entity entity, bool enabled)
    {
        scoreboardService.SetEnabledTo(entity.GetRequiredComponent<PlayerElementComponent>(), enabled);
    }

    public static void SetColumns(this ScoreboardService scoreboardService, Entity entity, List<ScoreboardColumn> columns)
    {
        scoreboardService.SetColumns(entity.GetRequiredComponent<PlayerElementComponent>(), columns);
    }

    public static void SetHeader(this ScoreboardService scoreboardService, Entity entity, ScoreboardHeader header)
    {
        scoreboardService.SetHeader(entity.GetRequiredComponent<PlayerElementComponent>(), header);
    }
}
