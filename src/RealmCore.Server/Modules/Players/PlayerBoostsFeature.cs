namespace RealmCore.Server.Modules.Players;

public sealed class ActiveBoostDto
{
    public required int BoostId { get; init; }
    public required int RemainingTime { get; init; }

    [return: NotNullIfNotNull(nameof(userActiveBoostData))]
    public static ActiveBoostDto? Map(UserActiveBoostData? userActiveBoostData, int remainingTime)
    {
        if (userActiveBoostData == null)
            return null;

        return new()
        {
            BoostId = userActiveBoostData.BoostId,
            RemainingTime = remainingTime
        };
    }
}

public interface IPlayerBoostsFeature : IPlayerFeature
{
    int[] AllBoosts { get; }
    IEnumerable<ActiveBoostDto> ActiveBoosts { get; }

    void AddBoost(int boostId);
    bool IsActive(int boostId);
    bool TryActivateBoost(int boostId, TimeSpan activeFor, bool force = false);
    bool TryRemoveBoost(int boostId);
}

internal sealed class PlayerBoostsFeature : IPlayerBoostsFeature, IUsesUserPersistentData, IDisposable
{
    private readonly object _lock = new();
    private readonly IDateTimeProvider _dateTimeProvider;
    private DateTime _sessionStartedDateTime;
    private ICollection<UserBoostData> _boosts = [];
    private ICollection<UserActiveBoostData> _activeBoosts = [];
    private bool _disposed;

    public RealmPlayer Player { get; }
    public event Action? VersionIncreased;

    public int[] AllBoosts
    {
        get
        {
            lock (_lock)
            {
                return _boosts.Select(x => x.BoostId).ToArray();
            }
        }
    }

    public IEnumerable<ActiveBoostDto> ActiveBoosts
    {
        get
        {
            var now = _dateTimeProvider.Now;
            var sessionElapsedTime = SessionElapsedTime;
            lock (_lock)
            {
                foreach (var activeBoostData in _activeBoosts)
                {
                    int elapsedTime;
                    if (activeBoostData.ActivatedAt >= _sessionStartedDateTime)
                    {
                        elapsedTime = (int)(now - activeBoostData.ActivatedAt).TotalSeconds;
                    }
                    else
                    {
                        elapsedTime = sessionElapsedTime;
                    }
                    var remainingTime = activeBoostData.RemainingTime - elapsedTime;
                    if(remainingTime > 0)
                        yield return ActiveBoostDto.Map(activeBoostData, remainingTime);
                }
            }
        }
    }

    public int SessionElapsedTime => (int)(_dateTimeProvider.Now - _sessionStartedDateTime).TotalSeconds;

    public PlayerBoostsFeature(PlayerContext playerContext, IDateTimeProvider dateTimeProvider)
    {
        Player = playerContext.Player;
        _dateTimeProvider = dateTimeProvider;
        _sessionStartedDateTime = _dateTimeProvider.Now;
    }

    public void LogIn(UserData userData)
    {
        lock (_lock)
        {
            _sessionStartedDateTime = _dateTimeProvider.Now;
            if (userData.Boosts != null)
                _boosts = userData.Boosts;
            if (userData.ActiveBoosts != null)
                _activeBoosts = userData.ActiveBoosts;
        }
    }

    private bool TryRemoveBoostCore(int boostId)
    {
        var boost = _boosts.Where(x => x.BoostId == boostId).FirstOrDefault();
        if (boost != null)
        {
            _boosts.Remove(boost);
            return true;
        }
        return false;
    }

    public void AddBoost(int boostId)
    {
        lock (_lock)
        {
            _boosts.Add(new UserBoostData
            {
                BoostId = boostId,
            });
        }
    }
    
    public bool TryActivateBoost(int boostId, TimeSpan activeFor, bool force = false)
    {
        lock (_lock)
        {
            if (IsActiveCore(boostId))
                return false;

            if(TryRemoveBoostCore(boostId) || force)
            {
                var now = _dateTimeProvider.Now;
                _activeBoosts.Add(new UserActiveBoostData
                {
                    BoostId = boostId,
                    ActivatedAt = now,
                    RemainingTime = (int)activeFor.TotalSeconds
                });
                return true;
            }
            return false;
        }
    }
    
    public bool TryRemoveBoost(int boostId)
    {
        lock (_lock)
        {
            var boost = _boosts.Where(x => x.BoostId == boostId).FirstOrDefault();
            if(boost != null)
            {
                _boosts.Remove(boost);
                return true;
            }
            return false;
        }
    }

    private bool IsActiveCore(int boostId)
    {
        var activeBoostData = _activeBoosts.Where(x => x.BoostId == boostId).FirstOrDefault();
        if (activeBoostData == null)
            return false;

        return GetRemainingTimeCore(activeBoostData) > 0;
    }
    
    public bool IsActive(int boostId)
    {
        lock (_lock)
            return IsActiveCore(boostId);
    }
    
    private int GetRemainingTimeCore(UserActiveBoostData activeBoostData)
    {
        var now = _dateTimeProvider.Now;
        lock (_lock)
        {
            int elapsedTime;
            if (activeBoostData.ActivatedAt >= _sessionStartedDateTime)
            {
                elapsedTime = (int)(now - activeBoostData.ActivatedAt).TotalSeconds;
            }
            else
            {
                elapsedTime = SessionElapsedTime;
            }
            var remainingTime = activeBoostData.RemainingTime - elapsedTime;
            return remainingTime;
        }
    }

    public int? GetRemainingTime(int boostId)
    {
        var now = _dateTimeProvider.Now;
        lock (_lock)
        {
            var activeBoostData = _activeBoosts.Where(x => x.BoostId == boostId).FirstOrDefault();
            if (activeBoostData == null)
                return null;

            return GetRemainingTimeCore(activeBoostData);
        }
    }

    public void Dispose()
    {
        lock (_lock)
        {
            if (_disposed)
                return;

            _disposed = true;
            foreach (var activeBoostData in _activeBoosts.ToArray())
            {
                var remainingTime = GetRemainingTimeCore(activeBoostData);
                if(remainingTime > 0)
                {
                    activeBoostData.RemainingTime = remainingTime;
                }
                else
                {
                    _activeBoosts.Remove(activeBoostData);
                }
            }
        }
    }
}
