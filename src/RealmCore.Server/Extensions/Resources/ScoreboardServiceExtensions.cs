using RealmCore.ECS;
using SlipeServer.Resources.Scoreboard;

namespace RealmCore.Server.Extensions.Resources;

public static class ScoreboardServiceExtensions
{
    public static void SetEnabledTo(this ScoreboardService scoreboardService, Entity entity, bool enabled)
    {
        scoreboardService.SetEnabledTo(entity.GetPlayer(), enabled);
    }

    public static void SetColumns(this ScoreboardService scoreboardService, Entity entity, List<ScoreboardColumn> columns)
    {
        scoreboardService.SetColumns(entity.GetPlayer(), columns);
    }

    public static void SetHeader(this ScoreboardService scoreboardService, Entity entity, ScoreboardHeader header)
    {
        scoreboardService.SetHeader(entity.GetPlayer(), header);
    }
}
