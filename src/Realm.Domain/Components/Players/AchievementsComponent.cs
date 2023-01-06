using Newtonsoft.Json.Linq;
using AchievementData = Realm.Persistance.Data.Achievement;

namespace Realm.Domain.Components.Players;

public class AchievementsComponent : Component
{
    public class Achievement
    {
        public float Progress { get; set; }
        public object? Value { get; set; }
        public bool PrizeReceived { get; set; }
    }

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
            Progress = x.Progress,
            Value = JsonConvert.DeserializeObject(x.Value),
            PrizeReceived = x.PrizeReceived,
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

        _achievements[achievementName].Value = value;
    }
    
    public T? GetAchievementValue<T>(string achievementName)
    {
        TryInitializeAchievement(achievementName);

        var value = _achievements[achievementName].Value;
        return (T?)value;
    }
    
    public float GetProgress(string achievementName)
    {
        TryInitializeAchievement(achievementName);

        return _achievements[achievementName].Progress;
    }
    
    public bool HasProgress(string achievementName, float progress)
    {
        TryInitializeAchievement(achievementName);

        return progress <= _achievements[achievementName].Progress;
    }

    public async Task<bool> TryReceiveReward(string achievementName, Func<Task> action)
    {
        TryInitializeAchievement(achievementName);

        var achievement = _achievements[achievementName];
        if (achievement.PrizeReceived)
            return false;

        achievement.PrizeReceived = true;
        await action();
        return true;
    }

    public bool UpdateProgress(string achievementName, float progress, float maximumProgress)
    {
        TryInitializeAchievement(achievementName);

        if (!_achievements.ContainsKey(achievementName))
            _achievements[achievementName] = new();
        var achievement = _achievements[achievementName];
        if (achievement.PrizeReceived || HasProgress(achievementName, maximumProgress))
            return false;

        achievement.Progress = Math.Min(progress + achievement.Progress, maximumProgress);
        if (HasProgress(achievementName, maximumProgress))
            AchievementUnlocked?.Invoke(Entity, achievementName);

        return true;
    }
}
