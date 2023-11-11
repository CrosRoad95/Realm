using System.Collections;

namespace RealmCore.Server.Services.Players;

internal class PlayerBansService : IPlayerBansService
{
    private ICollection<BanData> _bans = [];
    private readonly object _lock = new();
    private readonly IPlayerUserService _playerUserService;
    private readonly IDateTimeProvider _dateTimeProvider;

    public event Action<IPlayerBansService, BanDTO>? Added;
    public event Action<IPlayerBansService, BanDTO>? Removed;

    public RealmPlayer Player { get; }
    public PlayerBansService(PlayerContext playerContext, IPlayerUserService playerUserService, IDateTimeProvider dateTimeProvider)
    {
        Player = playerContext.Player;
        playerUserService.SignedIn += HandleSignedIn;
        playerUserService.SignedOut += HandleSignedOut;
        _playerUserService = playerUserService;
        _dateTimeProvider = dateTimeProvider;
    }

    private void HandleSignedIn(IPlayerUserService playerUserService, RealmPlayer _)
    {
        lock (_lock)
            _bans = playerUserService.User.Bans;
    }

    private void HandleSignedOut(IPlayerUserService playerUserService, RealmPlayer _)
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

        _playerUserService.IncreaseVersion();
        Added?.Invoke(this, BanDTO.Map(banData));
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
        lock (_lock)
        {
            ban = _bans.FirstOrDefault(x => x.Id == banId && x.Active);
            if (ban != null)
                removed = _bans.Remove(ban);
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

    public BanDTO? TryGet(int type)
    {
        var now = _dateTimeProvider.Now;
        lock (_lock)
        {
            foreach (var banDTO in _bans)
            {
                if (banDTO.Type == type && now > banDTO.End)
                {
                    return BanDTO.Map(banDTO);
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

    public IEnumerator<BanDTO> GetEnumerator()
    {
        var now = _dateTimeProvider.Now;
        lock (_lock)
            return new List<BanDTO>(_bans.Where(x => x.End > now && x.Active).Select(BanDTO.Map)).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
