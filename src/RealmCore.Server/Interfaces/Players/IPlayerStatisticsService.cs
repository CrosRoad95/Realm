
namespace RealmCore.Server.Interfaces.Players;

public interface IPlayerStatisticsService : IPlayerService, IEnumerable<UserStatDTO>
{
    IReadOnlyList<int> StatsIds { get; }

    event Action<IPlayerStatisticsService, int, float>? Decreased;
    event Action<IPlayerStatisticsService, int, float>? Increased;

    void Increase(int statId, float value = 1);
    void Decrease(int statId, float value = 1);
    void Set(int statId, float value);
    float Get(int statId);
}
