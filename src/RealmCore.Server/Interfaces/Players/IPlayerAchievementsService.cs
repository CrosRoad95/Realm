
namespace RealmCore.Server.Interfaces.Players;

public interface IPlayerAchievementsService : IPlayerService, IEnumerable<AchievementDTO>
{
    event Action<IPlayerAchievementsService, int>? Unlocked;
    event Action<IPlayerAchievementsService, int, float>? Progressed;

    T? GetAchievementValue<T>(int achievementId);
    float Get(int achievementId);
    bool HasReachedProgressThreshold(int achievementId, float progress);
    bool SetProgress(int achievementId, float progress, float maximumProgress);
    void SetValue(int achievementId, object value);
    bool TryReceiveReward(int achievementId, float requiredProgress);
    bool UpdateProgress(int achievementId, float progress, float maximumProgress);
    bool IsRewardReceived(int achievementId);
}
