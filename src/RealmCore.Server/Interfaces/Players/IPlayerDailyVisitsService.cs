
namespace RealmCore.Server.Interfaces.Players;

public interface IPlayerDailyVisitsService
{
    DateTime LastVisit { get; set; }
    int VisitsInRow { get; set; }
    int VisitsInRowRecord { get; set; }
    RealmPlayer Player { get; }

    event Action<IPlayerDailyVisitsService, int, bool>? PlayerVisited;
    event Action<IPlayerDailyVisitsService, int>? PlayerVisitsRecord;

    internal void Update(DateTime now);
}
