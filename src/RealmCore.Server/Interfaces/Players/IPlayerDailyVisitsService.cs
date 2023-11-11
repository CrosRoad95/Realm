namespace RealmCore.Server.Interfaces.Players;

public interface IPlayerDailyVisitsService : IPlayerService
{
    DateTime LastVisit { get; set; }
    int VisitsInRow { get; set; }
    int VisitsInRowRecord { get; set; }

    event Action<IPlayerDailyVisitsService, int, bool>? Visited;
    event Action<IPlayerDailyVisitsService, int>? VisitsRecord;

    internal void Update(DateTime now);
}
