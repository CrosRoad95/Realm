using AchievementData = Realm.Persistance.Data.Achievement;
using Achievement = Realm.Domain.Concepts.Achievement;

namespace Realm.Domain.Components.Players;

[ComponentUsage(false)]
public class AchievementsComponent : Component
{
    private readonly Dictionary<string, Achievement> _achievements = new();
    private readonly object _achievementsLock = new object();

    public IReadOnlyDictionary<string, Achievement> Achievements => _achievements;

    public event Action<AchievementsComponent, string>? AchievementUnlocked;
    public event Action<AchievementsComponent, string, float>? AchievementProgressed;

    public AchievementsComponent()
    {

    }

    internal AchievementsComponent(ICollection<AchievementData> achievements)
    {
        _achievements = achievements.ToDictionary(x => x.Name, x => new Achievement
        {
            progress = x.Progress,
            value = JsonConvert.DeserializeObject(x.Value),
            prizeReceived = x.PrizeReceived,
        });
    }

    private void TryInitializeAchievementInternal(string achievementName)
    {
        if (!_achievements.ContainsKey(achievementName))
            _achievements[achievementName] = new();
    }

    public void SetAchievementValue(string achievementName, object value)
    {
        lock (_achievementsLock)
        {
            TryInitializeAchievementInternal(achievementName);

            var achievement = _achievements[achievementName];
            achievement.value = value;
            _achievements[achievementName] = achievement;
        }
    }
    
    public T? GetAchievementValue<T>(string achievementName)
    {
        lock (_achievementsLock)
        {
            TryInitializeAchievementInternal(achievementName);

            var value = _achievements[achievementName].value;
            return (T?)value;
        }
    }
    
    public float GetProgress(string achievementName)
    {
        lock (_achievementsLock)
        {
            TryInitializeAchievementInternal(achievementName);
            return _achievements[achievementName].progress;
        }
    }
    
    public bool HasReachedProgressThreshold(string achievementName, float progress)
    {
        lock (_achievementsLock)
        {
            TryInitializeAchievementInternal(achievementName);

            return progress <= _achievements[achievementName].progress;
        }
    }

    public bool TryReceiveReward(string achievementName)
    {
        lock (_achievementsLock)
        {
            TryInitializeAchievementInternal(achievementName);

            var achievement = _achievements[achievementName];
            if (achievement.prizeReceived)
                return false;

            achievement.prizeReceived = true;
        }

        return true;
    }

    public bool UpdateProgress(string achievementName, float progress, float maximumProgress)
    {
        lock (_achievementsLock)
        {
            TryInitializeAchievementInternal(achievementName);

            var achievement = _achievements[achievementName];
            if (achievement.prizeReceived || HasReachedProgressThreshold(achievementName, maximumProgress))
                return false;

            achievement.progress = Math.Min(progress + achievement.progress, maximumProgress);
            if (HasReachedProgressThreshold(achievementName, maximumProgress))
                AchievementUnlocked?.Invoke(this, achievementName);
            else
                AchievementProgressed?.Invoke(this, achievementName, achievement.progress);

            return true;
        }
    }
}
