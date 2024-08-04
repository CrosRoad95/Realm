using static RealmCore.Server.Modules.Players.PlayersResults;

namespace RealmCore.Server.Modules.Players.Bans;

public sealed class PlayerBansFeature : IPlayerFeature, IEnumerable<UserBanDto>, IUsesUserPersistentData
{
    private readonly SemaphoreSlim _lock = new(1);
    private ICollection<UserBanData> _bans = [];
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IDb _db;

    public event Action<PlayerBansFeature, UserBanDto>? Added;
    public event Action<PlayerBansFeature, UserBanDto>? Deactivated;
    public event Action? VersionIncreased;

    public UserBanDto[] ActiveBans
    {
        get
        {
            _lock.Wait();
            try
            {
                var now = _dateTimeProvider.Now;
                return _bans.Where(x => x.IsActive(now)).Select(UserBanDto.Map).ToArray();
            }
            finally
            {
                _lock.Release();
            }
        }
    }

    public RealmPlayer Player { get; init; }
    public PlayerBansFeature(PlayerContext playerContext, IDateTimeProvider dateTimeProvider, IDb db)
    {
        Player = playerContext.Player;
        _dateTimeProvider = dateTimeProvider;
        _db = db;
    }

    public void LogIn(UserData userData)
    {
        _lock.Wait();
        try
        {
            _bans = userData.Bans;
        }
        finally
        {
            _lock.Release();
        }
    }

    public bool Add(int type, DateTime? until = null, string? reason = null, string? responsible = null)
    {
        var banData = new UserBanData
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
            var ban = InternalGetBanByType(type);
            var success = ban.Match(_ => false, notFound =>
            {
                _bans.Add(banData);
                VersionIncreased?.Invoke();
                Added?.Invoke(this, UserBanDto.Map(banData));
                return true;
            });

            return success;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<OneOf<UserBanDto[], NoBans>> FetchMore(int count = 10, CancellationToken cancellationToken = default)
    {
        var last = _bans.LastOrDefault();
        if (last == null)
            return new NoBans();

        await _lock.WaitAsync(cancellationToken);
        try
        {
            var query = _db.Bans
                .Where(x => x.UserId == Player.UserId && x.Id < last.Id)
                .OrderByDescending(x => x.Id)
                .Take(count);

            var results = await query.ToArrayAsync(cancellationToken);
            if (results.Length == 0)
                return new NoBans();

            return results.Select(UserBanDto.Map).ToArray();
        }
        finally
        {
            _lock.Release();
        }
    }

    public bool DeactivateBanById(int banId)
    {
        UserBanData? ban;
        bool removed = false;
        _lock.Wait();
        try
        {
            ban = _bans.FirstOrDefault(x => x.Id == banId && x.Active);
            if (ban != null)
            {
                ban.Active = false;
                removed = true;
            }
        }
        finally
        {
            _lock.Release();
        }

        if (removed && ban != null)
        {
            VersionIncreased?.Invoke();
            Deactivated?.Invoke(this, UserBanDto.Map(ban));
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
            return _bans.Any(x => x.Type == type && x.IsActive(now));
        }
        finally
        {
            _lock.Release();
        }
    }

    private OneOf<UserBanData, BanOfGivenTypeNotFound> InternalGetBanByType(int type)
    {
        var now = _dateTimeProvider.Now;
        var ban = _bans.FirstOrDefault(x => x.Type == type && x.IsActive(now));
        if (ban == null)
            return new BanOfGivenTypeNotFound();
        return ban;
    }
    
    public OneOf<UserBanDto, BanOfGivenTypeNotFound> GetBanByType(int type)
    {
        var now = _dateTimeProvider.Now;
        _lock.Wait();
        try
        {
            var banDto = InternalGetBanByType(type).Match(UserBanDto.Map, notFound => null);
            if(banDto == null)
                return new BanOfGivenTypeNotFound();
            return banDto;
        }
        finally
        {
            _lock.Release();
        }
    }

    public IEnumerator<UserBanDto> GetEnumerator()
    {
        UserBanData[] view;
        lock (_lock)
            view = [.. _bans];

        foreach (var settingData in view)
        {
            yield return UserBanDto.Map(settingData);
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
