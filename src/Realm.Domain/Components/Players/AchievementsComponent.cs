using AchievementData = Realm.Persistance.Data.Achievement;
using Achievement = Realm.Domain.Concepts.Achievement;

namespace Realm.Domain.Components.Players;

public class AchievementsComponent : Component
{
    private readonly Dictionary<string, Achievement> _achievements = new();
    public IReadOnlyDictionary<string, Achievement> Achievements => _achievements;

    public event Action<Entity, string>? AchievementUnlocked;

    public AchievementsComponent()
    {

    }

    public AchievementsComponent(ICollection<AchievementData> achievements)
    {
        _achievements = achievements.ToDictionary(x => x.Name, x => new Achievement
        {
            progress = x.Progress,
            value = JsonConvert.DeserializeObject(x.Value),
            prizeReceived = x.PrizeReceived,
        });
    }

    private void TryInitializeAchievement(string achievementName)
    {
        if (!_achievements.ContainsKey(achievementName))
            _achievements[achievementName] = new();
    }

    public void SetAchievementValue(string achievementName, object value)
    {
        TryInitializeAchievement(achievementName);

        var achievement = _achievements[achievementName];
        achievement.value = value;
        _achievements[achievementName] = achievement;
    }
    
    public T? GetAchievementValue<T>(string achievementName)
    {
        TryInitializeAchievement(achievementName);

        var value = _achievements[achievementName].value;
        return (T?)value;
    }
    
    public float GetProgress(string achievementName)
    {
        TryInitializeAchievement(achievementName);

        return _achievements[achievementName].progress;
    }
    
    public bool HasProgress(string achievementName, float progress)
    {
        TryInitializeAchievement(achievementName);

        return progress <= _achievements[achievementName].progress;
    }

    public async Task<bool> TryReceiveReward(string achievementName, Func<Task> action)
    {
        TryInitializeAchievement(achievementName);

        var achievement = _achievements[achievementName];
        if (achievement.prizeReceived)
            return false;

        achievement.prizeReceived = true;
        await action();
        return true;
    }

    public bool UpdateProgress(string achievementName, float progress, float maximumProgress)
    {
        TryInitializeAchievement(achievementName);

        if (!_achievements.ContainsKey(achievementName))
            _achievements[achievementName] = new();
        var achievement = _achievements[achievementName];
        if (achievement.prizeReceived || HasProgress(achievementName, maximumProgress))
            return false;

        achievement.progress = Math.Min(progress + achievement.progress, maximumProgress);
        if (HasProgress(achievementName, maximumProgress))
            AchievementUnlocked?.Invoke(Entity, achievementName);

        return true;
    }
}
