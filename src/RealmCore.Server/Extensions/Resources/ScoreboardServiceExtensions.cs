using SlipeServer.Resources.Scoreboard;

namespace RealmCore.Server.Extensions.Resources;

public static class ScoreboardServiceExtensions
{
    public static void SetEnabledTo(this ScoreboardService scoreboardService, Entity entity, bool enabled)
    {
        scoreboardService.SetEnabledTo(entity.Player, enabled);
    }

    public static void SetColumns(this ScoreboardService scoreboardService, Entity entity, List<ScoreboardColumn> columns)
    {
        scoreboardService.SetColumns(entity.Player, columns);
    }

    public static void SetHeader(this ScoreboardService scoreboardService, Entity entity, ScoreboardHeader header)
    {
        scoreboardService.SetHeader(entity.Player, header);
    }
}
