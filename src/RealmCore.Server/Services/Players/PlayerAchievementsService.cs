namespace RealmCore.Server.Services.Players;

internal class PlayerAchievementsService : IPlayerAchievementsService
{
    private ICollection<AchievementData> _achievements = [];
    private readonly object _lock = new();

    public event Action<IPlayerAchievementsService, int>? Unlocked;
    public event Action<IPlayerAchievementsService, int, float>? Progressed;

    public RealmPlayer Player { get; private set; }
    public PlayerAchievementsService(PlayerContext playerContext, IPlayerUserService playerUserService)
    {
        Player = playerContext.Player;
        playerUserService.SignedIn += HandleSignedIn;
        playerUserService.SignedOut += HandleSignedOut;
    }

    private void HandleSignedIn(IPlayerUserService playerUserService)
    {
        lock (_lock)
            _achievements = playerUserService.User.Achievements;
    }

    private void HandleSignedOut(IPlayerUserService playerUserService)
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
            return JsonConvert.DeserializeObject<T?>(achievement.Value);
        }
    }

    public float Get(int achievementId)
    {
        lock (_lock)
        {
            var achievement = GetById(achievementId);
            return achievement.Progress;
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

    public bool TryReceiveReward(int achievementId, float requiredProgress)
    {
        if (requiredProgress < 0)
            throw new ArgumentOutOfRangeException(nameof(requiredProgress));

        lock (_lock)
        {
            var achievement = GetById(achievementId);

            if (achievement.PrizeReceived || achievement.Progress < requiredProgress)
                return false;

            achievement.PrizeReceived = true;
            return true;
        }
    }

    public bool UpdateProgress(int achievementId, float progress, float maximumProgress)
    {
        return SetProgress(achievementId, Math.Min(progress + Get(achievementId), maximumProgress), maximumProgress);
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
            if (achievement.PrizeReceived || HasReachedProgressThreshold(achievementId, maximumProgress))
                return false;

            achievement.Progress = Math.Min(progress, maximumProgress);
            if (HasReachedProgressThreshold(achievementId, maximumProgress))
                Unlocked?.Invoke(this, achievementId);
            else
                Progressed?.Invoke(this, achievementId, achievement.Progress);

            return true;
        }
    }

    private static AchievementDTO Map(AchievementData? achievementData)
    {
        if (achievementData == null)
            return null;

        return new AchievementDTO
        {
            AchievementId = achievementData.AchievementId,
            PrizeReceived = achievementData.PrizeReceived,
            Progress = achievementData.Progress,
            Value = achievementData.Value
        };
    }

    public IEnumerator<AchievementDTO> GetEnumerator()
    {
        lock (_lock)
            return new List<AchievementDTO>(_achievements.Select(Map)).GetEnumerator();
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
}
