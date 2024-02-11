namespace RealmCore.Server.Modules.Players.Achievements;

public interface IPlayerAchievementsFeature : IPlayerFeature, IEnumerable<AchievementDto>
{
    event Action<IPlayerAchievementsFeature, int>? Unlocked;
    event Action<IPlayerAchievementsFeature, int, float>? Progressed;

    T? GetAchievementValue<T>(int achievementId);
    AchievementDto Get(int achievementId);
    bool HasReachedProgressThreshold(int achievementId, float progress);
    bool SetProgress(int achievementId, float progress, float maximumProgress);
    void SetValue(int achievementId, object value);
    bool TryReceiveReward(int achievementId, float requiredProgress, DateTime now);
    bool UpdateProgress(int achievementId, float progress, float maximumProgress);
    bool IsRewardReceived(int achievementId);
    float GetProgress(int achievementId);
}

internal sealed class PlayerAchievementsFeature : IPlayerAchievementsFeature
{
    private readonly object _lock = new();
    private ICollection<AchievementData> _achievements = [];
    private readonly IPlayerUserFeature _playerUserFeature;

    public event Action<IPlayerAchievementsFeature, int>? Unlocked;
    public event Action<IPlayerAchievementsFeature, int, float>? Progressed;

    public RealmPlayer Player { get; init; }
    public PlayerAchievementsFeature(PlayerContext playerContext, IPlayerUserFeature playerUserFeature)
    {
        Player = playerContext.Player;
        playerUserFeature.SignedIn += HandleSignedIn;
        playerUserFeature.SignedOut += HandleSignedOut;
        _playerUserFeature = playerUserFeature;
    }

    private void HandleSignedIn(IPlayerUserFeature playerUserFeature, RealmPlayer _)
    {
        lock (_lock)
            _achievements = playerUserFeature.User.Achievements;
    }

    private void HandleSignedOut(IPlayerUserFeature playerUserFeature, RealmPlayer _)
    {
        lock (_lock)
            _achievements = [];
    }

    private AchievementData GetById(int id)
    {
        var stat = _achievements.FirstOrDefault(x => x.AchievementId == id);
        if (stat == null)
        {
            stat = new AchievementData
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
        }
    }

    public T? GetAchievementValue<T>(int achievementId)
    {
        lock (_lock)
        {
            var achievement = GetById(achievementId);
            if (string.IsNullOrEmpty(achievement.Value))
                return default;
            return JsonConvert.DeserializeObject<T?>(achievement.Value);
        }
    }

    public AchievementDto Get(int achievementId)
    {
        lock (_lock)
        {
            var achievement = GetById(achievementId);
            return AchievementDto.Map(achievement);
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
            _playerUserFeature.IncreaseVersion();
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

            return true;
        }
    }

    public IEnumerator<AchievementDto> GetEnumerator()
    {
        lock (_lock)
            return new List<AchievementDto>(_achievements.Select(AchievementDto.Map)).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
