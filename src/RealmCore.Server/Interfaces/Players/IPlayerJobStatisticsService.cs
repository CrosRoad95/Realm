namespace RealmCore.Server.Interfaces.Players;

public interface IPlayerJobStatisticsService : IPlayerService
{
    void AddPoints(short jobId, ulong points);
    void AddTimePlayed(short jobId, ulong timePlayed);
    (ulong, ulong) GetTotalPoints(short jobId);
}
