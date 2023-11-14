using System.Collections;

namespace RealmCore.Server.Services.Players;

internal class PlayerBansService : IPlayerBansService
{
    private readonly SemaphoreSlim _lock = new(1);
    private ICollection<BanData> _bans = [];
    private readonly IPlayerUserService _playerUserService;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IDb _db;

    public event Action<IPlayerBansService, BanDTO>? Added;
    public event Action<IPlayerBansService, BanDTO>? Removed;

    public RealmPlayer Player { get; }
    public PlayerBansService(PlayerContext playerContext, IPlayerUserService playerUserService, IDateTimeProvider dateTimeProvider, IDb db)
    {
        Player = playerContext.Player;
        playerUserService.SignedIn += HandleSignedIn;
        playerUserService.SignedOut += HandleSignedOut;
        _playerUserService = playerUserService;
        _dateTimeProvider = dateTimeProvider;
        _db = db;
    }

    private void HandleSignedIn(IPlayerUserService playerUserService, RealmPlayer _)
    {
        _lock.Wait();
        try
        {
            _bans = playerUserService.User.Bans;
        }
        finally
        {
            _lock.Release();
        }
    }

    private void HandleSignedOut(IPlayerUserService playerUserService, RealmPlayer _)
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

        _playerUserService.IncreaseVersion();
        Added?.Invoke(this, BanDTO.Map(banData));
    }

    public async Task<List<BanDTO>> FetchMore(int count = 10, CancellationToken cancellationToken = default)
    {
        var last = _bans.LastOrDefault();
        if (last == null)
            return [];


        await _lock.WaitAsync(cancellationToken);
        try
        {
            var query = _db.Bans
                .Where(x => x.UserId == Player.UserId && x.Id < last.Id)
                .OrderByDescending(x => x.Id)
                .Take(count);

            var results = await query.ToListAsync(cancellationToken);

            return results.Select(BanDTO.Map).ToList();
        }
        finally
        {
            _lock.Release();
        }
    }

    public bool RemoveByType(int type)
    {
        BanData? ban;
        bool removed = false;
        _lock.Wait();
        try
        {
            ban = GetBanByType(type);
            if (ban != null)
                removed = _bans.Remove(ban);
        }
        finally
        {
            _lock.Release();
        }

        if (removed && ban != null)
        {
            _playerUserService.IncreaseVersion();
            Removed?.Invoke(this, BanDTO.Map(ban));
            return true;
        }
        return false;
    }

    public bool RemoveById(int banId)
    {
        BanData? ban;
        bool removed = false;
        _lock.Wait();
        try
        {
            ban = _bans.FirstOrDefault(x => x.Id == banId && x.Active);
            if (ban != null)
                removed = _bans.Remove(ban);
        }
        finally
        {
            _lock.Release();
        }

        if (removed && ban != null)
        {
            _playerUserService.IncreaseVersion();
            Removed?.Invoke(this, BanDTO.Map(ban));
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
                if (ban.Active && ban.Type == type && now < ban.End)
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

    public BanDTO? TryGet(int type)
    {
        var now = _dateTimeProvider.Now;
        _lock.Wait();
        try
        {
            foreach (var banDTO in _bans)
            {
                if (banDTO.Type == type && now > banDTO.End)
                {
                    return BanDTO.Map(banDTO);
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
        return _bans.FirstOrDefault(x => x.Type == type && x.End < now && x.Active);
    }

    public IEnumerator<BanDTO> GetEnumerator()
    {
        var now = _dateTimeProvider.Now;
        lock (_lock)
            return new List<BanDTO>(_bans.Where(x => x.End > now && x.Active).Select(BanDTO.Map)).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
