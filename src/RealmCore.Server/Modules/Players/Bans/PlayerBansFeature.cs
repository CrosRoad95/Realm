namespace RealmCore.Server.Modules.Players.Bans;

public interface IPlayerBansFeature : IPlayerFeature, IEnumerable<BanDto>
{
    /// <summary>
    /// Return all active bans
    /// </summary>
    BanDto[] ActiveBans { get; }

    event Action<IPlayerBansFeature, BanDto>? Added;
    event Action<IPlayerBansFeature, BanDto>? Deactivated;

    void Add(int type, DateTime? until = null, string? reason = null, string? responsible = null);
    /// <summary>
    /// Fetches X more bans from database, returning empty list when all bans were fetched.
    /// </summary>
    Task<List<BanDto>> FetchMore(int count = 10, CancellationToken cancellationToken = default);
    /// <summary>
    /// Return true if ban of given type is currently active
    /// </summary>
    bool IsBanned(int type);
    /// <summary>
    /// Deactivates ban by id
    /// </summary>
    bool DeactivateBanById(int banId);
    /// <summary>
    /// Return active ban or default by given type
    /// </summary>
    BanDto? GetByIdOrDefault(int type);
}

internal sealed class PlayerBansFeature : IPlayerBansFeature
{
    private readonly SemaphoreSlim _lock = new(1);
    private ICollection<BanData> _bans = [];
    private readonly IPlayerUserFeature _playerUserFeature;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IDb _db;

    public event Action<IPlayerBansFeature, BanDto>? Added;
    public event Action<IPlayerBansFeature, BanDto>? Deactivated;

    public BanDto[] ActiveBans
    {
        get
        {
            _lock.Wait();
            try
            {
                var now = _dateTimeProvider.Now;
                return _bans.Where(x => x.IsActive(now)).Select(BanDto.Map).ToArray();
            }
            finally
            {
                _lock.Release();
            }
        }
    }

    public RealmPlayer Player { get; init; }
    public PlayerBansFeature(PlayerContext playerContext, IPlayerUserFeature playerUserFeature, IDateTimeProvider dateTimeProvider, IDb db)
    {
        Player = playerContext.Player;
        playerUserFeature.SignedIn += HandleSignedIn;
        playerUserFeature.SignedOut += HandleSignedOut;
        _playerUserFeature = playerUserFeature;
        _dateTimeProvider = dateTimeProvider;
        _db = db;
    }

    private void HandleSignedIn(IPlayerUserFeature playerUserFeature, RealmPlayer _)
    {
        _lock.Wait();
        try
        {
            _bans = playerUserFeature.UserData.Bans;
        }
        finally
        {
            _lock.Release();
        }
    }

    private void HandleSignedOut(IPlayerUserFeature playerUserFeature, RealmPlayer _)
    {
        _lock.Wait();
        try
        {
            _bans = [];
        }
        finally
        {
            _lock.Release();
        }
    }

    public void Add(int type, DateTime? until = null, string? reason = null, string? responsible = null)
    {
        var banData = new BanData
        {
            Serial = Player.Client.GetSerial(),
            End = until ?? DateTime.MaxValue,
            Reason = reason,
            Responsible = responsible,
            Type = type,
            Active = true
        };

        _lock.Wait();
        try
        {
            var ban = GetBanByType(type);
            if (ban != null)
                throw new InvalidOperationException();

            _bans.Add(banData);
        }
        finally
        {
            _lock.Release();
        }

        _playerUserFeature.IncreaseVersion();
        Added?.Invoke(this, BanDto.Map(banData));
    }

    public async Task<List<BanDto>> FetchMore(int count = 10, CancellationToken cancellationToken = default)
    {
        var last = _bans.LastOrDefault();
        if (last == null)
            return [];


        await _lock.WaitAsync(cancellationToken);
        try
        {
            var query = _db.Bans
                .Where(x => x.UserId == Player.PersistentId && x.Id < last.Id)
                .OrderByDescending(x => x.Id)
                .Take(count);

            var results = await query.ToListAsync(cancellationToken);

            return results.Select(BanDto.Map).ToList();
        }
        finally
        {
            _lock.Release();
        }
    }

    public bool DeactivateBanById(int banId)
    {
        BanData? ban;
        bool removed = false;
        _lock.Wait();
        try
        {
            ban = _bans.FirstOrDefault(x => x.Id == banId && x.Active);
            if (ban != null)
            {
                ban.Active = false;
            }
        }
        finally
        {
            _lock.Release();
        }

        if (removed && ban != null)
        {
            _playerUserFeature.IncreaseVersion();
            Deactivated?.Invoke(this, BanDto.Map(ban));
            return true;
        }
        return false;
    }

    public bool IsBanned(int type)
    {
        var now = _dateTimeProvider.Now;
        _lock.Wait();
        try
        {
            foreach (var ban in _bans)
            {
                if (ban.Type == type && ban.IsActive(now))
                {
                    return true;
                }
            }
            return false;
        }
        finally
        {
            _lock.Release();
        }
    }

    public BanDto? GetByIdOrDefault(int type)
    {
        var now = _dateTimeProvider.Now;
        _lock.Wait();
        try
        {
            foreach (var ban in _bans)
            {
                if (ban.Type == type && ban.IsActive(now))
                {
                    return BanDto.Map(ban);
                }
            }
            return null;
        }
        finally
        {
            _lock.Release();
        }
    }

    private BanData? GetBanByType(int type)
    {
        var now = _dateTimeProvider.Now;
        return _bans.FirstOrDefault(x => x.Type == type && x.IsActive(now));
    }

    public IEnumerator<BanDto> GetEnumerator()
    {
        var now = _dateTimeProvider.Now;
        lock (_lock)
            return new List<BanDto>(_bans.Where(x => x.IsActive(now)).Select(BanDto.Map)).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
