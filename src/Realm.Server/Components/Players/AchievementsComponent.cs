using AchievementData = Realm.Persistance.Data.AchievementData;
using Achievement = Realm.Server.Concepts.Achievement;

namespace Realm.Server.Components.Players;

[ComponentUsage(false)]
public class AchievementsComponent : Component
{
    private readonly Dictionary<int, Achievement> _achievements = new();
    private readonly object _achievementsLock = new object();

    public IReadOnlyDictionary<int, Achievement> Achievements => _achievements;

    public event Action<AchievementsComponent, int>? AchievementUnlocked;
    public event Action<AchievementsComponent, int, float>? AchievementProgressed;

    public AchievementsComponent()
    {

    }

    internal AchievementsComponent(ICollection<AchievementData> achievements)
    {
        _achievements = achievements.ToDictionary(x => x.AchievementId, x => new Achievement
        {
            progress = x.Progress,
            value = JsonConvert.DeserializeObject(x.Value),
            prizeReceived = x.PrizeReceived,
        });
    }

    private void TryInitializeAchievementInternal(int achievementId)
    {
        if (!_achievements.ContainsKey(achievementId))
            _achievements[achievementId] = new();
    }

    public void SetAchievementValue(int achievementId, object value)
    {
        ThrowIfDisposed();

        lock (_achievementsLock)
        {
            TryInitializeAchievementInternal(achievementId);

            var achievement = _achievements[achievementId];
            achievement.value = value;
            _achievements[achievementId] = achievement;
        }
    }

    public T? GetAchievementValue<T>(int achievementId)
    {
        ThrowIfDisposed();

        lock (_achievementsLock)
        {
            TryInitializeAchievementInternal(achievementId);

            var value = _achievements[achievementId].value;
            return (T?)value;
        }
    }

    public float GetProgress(int achievementId)
    {
        ThrowIfDisposed();

        lock (_achievementsLock)
        {
            TryInitializeAchievementInternal(achievementId);
            return _achievements[achievementId].progress;
        }
    }

    public bool HasReachedProgressThreshold(int achievementId, float progress)
    {
        ThrowIfDisposed();

        if (progress < 0)
            throw new ArgumentOutOfRangeException(nameof(progress));

        lock (_achievementsLock)
        {
            TryInitializeAchievementInternal(achievementId);

            return progress <= _achievements[achievementId].progress;
        }
    }

    public bool TryReceiveReward(int achievementId, float requiredProgress)
    {
        ThrowIfDisposed();

        if (requiredProgress < 0)
            throw new ArgumentOutOfRangeException(nameof(requiredProgress));

        lock (_achievementsLock)
        {
            TryInitializeAchievementInternal(achievementId);

            var achievement = _achievements[achievementId];
            if (achievement.prizeReceived || achievement.progress < requiredProgress)
                return false;

            _achievements[achievementId] = _achievements[achievementId] with { prizeReceived = true };
        }

        return true;
    }

    public bool UpdateProgress(int achievementId, float progress, float maximumProgress)
    {
        ThrowIfDisposed();

        if (progress < 0)
            throw new ArgumentOutOfRangeException(nameof(progress));

        if (maximumProgress < 0)
            throw new ArgumentOutOfRangeException(nameof(maximumProgress));

        lock (_achievementsLock)
        {
            TryInitializeAchievementInternal(achievementId);

            var achievement = _achievements[achievementId];

            if (achievement.prizeReceived || HasReachedProgressThreshold(achievementId, maximumProgress))
                return false;

            _achievements[achievementId] = achievement with { progress = Math.Min(progress + achievement.progress, maximumProgress) };
            if (HasReachedProgressThreshold(achievementId, maximumProgress))
                AchievementUnlocked?.Invoke(this, achievementId);
            else
                AchievementProgressed?.Invoke(this, achievementId, _achievements[achievementId].progress);

            return true;
        }
    }
}
