
namespace RealmCore.Server.Interfaces.Players;

public interface IPlayerPlayTimeService : IPlayerService
{
    TimeSpan PlayTime { get; }
    TimeSpan TotalPlayTime { get; }

    event Action<IPlayerPlayTimeService>? MinutePlayed;
    event Action<IPlayerPlayTimeService>? MinuteTotalPlayed;

    void InternalSetTotalPlayTime(ulong time);
    void Reset();
}
