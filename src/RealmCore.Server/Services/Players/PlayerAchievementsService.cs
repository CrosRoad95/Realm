namespace RealmCore.Server.Services.Players;

public interface IPlayerAchievementsService : IPlayerService, IEnumerable<AchievementDTO>
{
    event Action<IPlayerAchievementsService, int>? Unlocked;
    event Action<IPlayerAchievementsService, int, float>? Progressed;

    T? GetAchievementValue<T>(int achievementId);
    AchievementDTO Get(int achievementId);
    bool HasReachedProgressThreshold(int achievementId, float progress);
    bool SetProgress(int achievementId, float progress, float maximumProgress);
    void SetValue(int achievementId, object value);
    bool TryReceiveReward(int achievementId, float requiredProgress, DateTime now);
    bool UpdateProgress(int achievementId, float progress, float maximumProgress);
    bool IsRewardReceived(int achievementId);
    float GetProgress(int achievementId);
}

internal class PlayerAchievementsService : IPlayerAchievementsService
{
    private ICollection<AchievementData> _achievements = [];
    private readonly object _lock = new();
    private readonly IPlayerUserService _playerUserService;

    public event Action<IPlayerAchievementsService, int>? Unlocked;
    public event Action<IPlayerAchievementsService, int, float>? Progressed;

    public RealmPlayer Player { get; private set; }
    public PlayerAchievementsService(PlayerContext playerContext, IPlayerUserService playerUserService)
    {
        Player = playerContext.Player;
        playerUserService.SignedIn += HandleSignedIn;
        playerUserService.SignedOut += HandleSignedOut;
        _playerUserService = playerUserService;
    }

    private void HandleSignedIn(IPlayerUserService playerUserService, RealmPlayer _)
    {
        lock (_lock)
            _achievements = playerUserService.User.Achievements;
    }

    private void HandleSignedOut(IPlayerUserService playerUserService, RealmPlayer _)
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

    public AchievementDTO Get(int achievementId)
    {
        lock (_lock)
        {
            var achievement = GetById(achievementId);
            return AchievementDTO.Map(achievement);
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
            _playerUserService.IncreaseVersion();
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

    public IEnumerator<AchievementDTO> GetEnumerator()
    {
        lock (_lock)
            return new List<AchievementDTO>(_achievements.Select(AchievementDTO.Map)).GetEnumerator();
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
}
