namespace RealmCore.Server.Modules.Players.Achievements;

public sealed class PlayerAchievementsFeature : IPlayerFeature, IEnumerable<UserAchievementDto>, IUsesUserPersistentData
{
    private readonly object _lock = new();
    private ICollection<UserAchievementData> _achievements = [];

    public event Action<PlayerAchievementsFeature, int>? Unlocked;
    public event Action<PlayerAchievementsFeature, int, float>? Progressed;
    public event Action? VersionIncreased;

    public RealmPlayer Player { get; init; }
    public PlayerAchievementsFeature(PlayerContext playerContext)
    {
        Player = playerContext.Player;
    }

    public void LogIn(UserData userData)
    {
        lock (_lock)
            _achievements = userData.Achievements;
    }

    private UserAchievementData GetById(int id)
    {
        var stat = _achievements.FirstOrDefault(x => x.AchievementId == id);
        if (stat == null)
        {
            stat = new UserAchievementData
            {
                AchievementId = id
            };
            _achievements.Add(stat);
            return stat;
        }
        return stat;
    }

    public void SetValue(int achievementId, object value)
    {
        lock (_lock)
        {
            var achievement = GetById(achievementId);
            achievement.Value = JsonConvert.SerializeObject(value);
            VersionIncreased?.Invoke();
        }
    }

    public T? GetValue<T>(int achievementId)
    {
        lock (_lock)
        {
            var achievement = GetById(achievementId);
            if (string.IsNullOrEmpty(achievement.Value))
                return default;
            return JsonConvert.DeserializeObject<T?>(achievement.Value);
        }
    }

    public UserAchievementDto Get(int achievementId)
    {
        lock (_lock)
        {
            var achievement = GetById(achievementId);
            return UserAchievementDto.Map(achievement);
        }
    }

    public bool HasReachedProgressThreshold(int achievementId, float progress)
    {
        if (progress < 0)
            throw new ArgumentOutOfRangeException(nameof(progress));

        lock (_lock)
        {
            var achievement = GetById(achievementId);
            return progress <= achievement.Progress;
        }
    }

    public bool TryReceiveReward(int achievementId, float requiredProgress, DateTime now)
    {
        if (requiredProgress < 0)
            throw new ArgumentOutOfRangeException(nameof(requiredProgress));

        lock (_lock)
        {
            var achievement = GetById(achievementId);

            if (achievement.PrizeReceivedDateTime != null || achievement.Progress < requiredProgress)
                return false;

            achievement.PrizeReceivedDateTime = now;
            VersionIncreased?.Invoke();
            return true;
        }
    }

    public bool IsRewardReceived(int achievementId)
    {
        lock (_lock)
        {
            var achievement = GetById(achievementId);

            return achievement.PrizeReceivedDateTime != null;
        }
    }

    public float GetProgress(int achievementId)
    {
        lock (_lock)
        {
            var achievement = GetById(achievementId);

            return achievement.Progress;
        }
    }

    public bool UpdateProgress(int achievementId, float progress, float maximumProgress)
    {
        return SetProgress(achievementId, Math.Min(progress + GetProgress(achievementId), maximumProgress), maximumProgress);
    }

    public bool SetProgress(int achievementId, float progress, float maximumProgress)
    {
        if (progress < 0)
            throw new ArgumentOutOfRangeException(nameof(progress));

        if (maximumProgress < 0)
            throw new ArgumentOutOfRangeException(nameof(maximumProgress));

        lock (_lock)
        {
            var achievement = GetById(achievementId);
            if (achievement.PrizeReceivedDateTime != null || HasReachedProgressThreshold(achievementId, maximumProgress))
                return false;

            achievement.Progress = Math.Min(progress, maximumProgress);
            if (HasReachedProgressThreshold(achievementId, maximumProgress))
                Unlocked?.Invoke(this, achievementId);
            else
                Progressed?.Invoke(this, achievementId, achievement.Progress);

            VersionIncreased?.Invoke();
            return true;
        }
    }

    public void Clear()
    {
        lock (_lock)
            _achievements.Clear();
    }
    public IEnumerator<UserAchievementDto> GetEnumerator()
    {
        UserAchievementData[] view;
        lock (_lock)
            view = [.. _achievements];

        foreach (var settingData in view)
        {
            yield return UserAchievementDto.Map(settingData);
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
