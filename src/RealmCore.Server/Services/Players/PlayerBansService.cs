namespace RealmCore.Server.Services.Players;

internal class PlayerBansService : IPlayerBansService, IDisposable
{
    private ICollection<BanData> _bans = [];
    private readonly object _lock = new();
    private readonly IDateTimeProvider _dateTimeProvider;

    public event Action<BanDTO>? Added;
    public event Action<BanDTO>? Removed;

    public IReadOnlyList<BanDTO> AllActive
    {
        get
        {
            var now = _dateTimeProvider.Now;
            lock (_lock)
                return new List<BanDTO>(_bans.Where(x => x.End > now).Select(Map).ToList());
        }
    }

    public RealmPlayer Player { get; }
    public PlayerBansService(PlayerContext playerContext, IPlayerUserService playerUserService, IDateTimeProvider dateTimeProvider)
    {
        Player = playerContext.Player;
        playerUserService.SignedIn += HandleSignedIn;
        playerUserService.SignedOut += HandleSignedOut;
        _dateTimeProvider = dateTimeProvider;
    }

    private void HandleSignedIn(IPlayerUserService playerUserService)
    {
        lock (_lock)
            _bans = playerUserService.User.Bans;
    }

    private void HandleSignedOut(IPlayerUserService playerUserService)
    {
        lock (_lock)
            _bans = [];
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

        lock (_lock)
        {
            var ban = GetBanByType(type);
            if (ban != null)
                throw new InvalidOperationException();

            _bans.Add(banData);
        }

        Added?.Invoke(Map(banData));
    }

    public bool RemoveByType(int type)
    {
        BanData? ban;
        bool removed = false;
        lock (_lock)
        {
            ban = GetBanByType(type);
            if (ban != null)
                removed = _bans.Remove(ban);
        }

        if (removed && ban != null)
        {
            Removed?.Invoke(Map(ban));
            return true;
        }
        return false;
    }

    public bool RemoveById(int banId)
    {
        BanData? ban;
        bool removed = false;
        lock (_lock)
        {
            ban = _bans.FirstOrDefault(x => x.Id == banId && x.Active);
            if (ban != null)
                removed = _bans.Remove(ban);
        }

        if (removed && ban != null)
        {
            Removed?.Invoke(Map(ban));
            return true;
        }
        return false;
    }

    public bool IsBanned(int type)
    {
        var now = _dateTimeProvider.Now;
        lock (_lock)
        {
            foreach (var ban in _bans)
            {
                if (ban.Active && ban.Type == type && now < ban.End)
                {
                    return true;
                }
            }
        }
        return false;
    }

    public BanDTO? TryGetBan(int type)
    {
        var now = _dateTimeProvider.Now;
        lock (_lock)
        {
            foreach (var banDTO in _bans)
            {
                if (banDTO.Type == type && now > banDTO.End)
                {
                    return Map(banDTO);
                }
            }
        }
        return null;
    }

    private BanData? GetBanByType(int type)
    {
        var now = _dateTimeProvider.Now;
        return _bans.FirstOrDefault(x => x.Type == type && x.End < now && x.Active);
    }

    private static BanDTO Map(BanData banData) => new()
    {
        Id = banData.Id,
        End = banData.End,
        UserId = banData.UserId,
        Reason = banData.Reason,
        Responsible = banData.Responsible,
        Serial = banData.Serial,
        Type = banData.Type
    };

    public void Dispose()
    {
    }
}
