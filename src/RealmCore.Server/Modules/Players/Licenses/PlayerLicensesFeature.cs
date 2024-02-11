namespace RealmCore.Server.Modules.Players.Licenses;

public interface IPlayerLicensesFeature : IPlayerFeature, IEnumerable<PlayerLicenseDto>
{
    event Action<IPlayerLicensesFeature, PlayerLicenseDto>? Added;
    event Action<IPlayerLicensesFeature, PlayerLicenseDto>? Suspended;
    event Action<IPlayerLicensesFeature, PlayerLicenseDto>? UnSuspended;

    PlayerLicenseDto? Get(int licenseId);
    bool Has(int licenseId, bool includeSuspended = false);
    bool IsSuspended(int licenseId);
    void Suspend(int licenseId, TimeSpan timeSpan, string? reason = null);
    bool TryAdd(int licenseId);
    void UnSuspend(int licenseId);
}

internal sealed class PlayerLicensesFeature : IPlayerLicensesFeature
{
    private readonly object _lock = new();
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IPlayerUserFeature _playerUserFeature;
    private ICollection<UserLicenseData> _licenses = [];

    public event Action<IPlayerLicensesFeature, PlayerLicenseDto>? Added;
    public event Action<IPlayerLicensesFeature, PlayerLicenseDto>? Suspended;
    public event Action<IPlayerLicensesFeature, PlayerLicenseDto>? UnSuspended;
    public RealmPlayer Player { get; init; }
    public PlayerLicensesFeature(PlayerContext playerContext, IDateTimeProvider dateTimeProvider, IPlayerUserFeature playerUserFeature)
    {
        Player = playerContext.Player;
        _dateTimeProvider = dateTimeProvider;
        _playerUserFeature = playerUserFeature;
        playerUserFeature.SignedIn += HandleSignedIn;
        playerUserFeature.SignedOut += HandleSignedOut;
    }

    private void HandleSignedIn(IPlayerUserFeature playerUserFeature, RealmPlayer _)
    {
        lock (_lock)
            _licenses = playerUserFeature.User.Licenses;
    }

    private void HandleSignedOut(IPlayerUserFeature playerUserFeature, RealmPlayer _)
    {
        lock (_lock)
            _licenses = [];
    }

    public PlayerLicenseDto Get(int licenseId)
    {
        lock (_lock)
        {
            var userLicenseData = InternalGetById(licenseId);
            return PlayerLicenseDto.Map(userLicenseData);
        }
    }

    public bool IsSuspended(int licenseId)
    {
        lock (_lock)
        {
            var userLicenseData = InternalGetById(licenseId);
            return userLicenseData.IsSuspended(_dateTimeProvider.Now);
        }
    }

    public bool TryAdd(int licenseId)
    {
        var userLicenseData = new UserLicenseData
        {
            LicenseId = licenseId,
        };

        lock (_lock)
        {
            if (InternalHas(licenseId, true))
                return false;

            _licenses.Add(userLicenseData);
            _playerUserFeature.IncreaseVersion();
            Added?.Invoke(this, PlayerLicenseDto.Map(userLicenseData));
            return true;
        }
    }

    public bool Has(int licenseId, bool includeSuspended = false)
    {
        lock (_lock)
            return InternalHas(licenseId, includeSuspended);
    }

    public void Suspend(int licenseId, TimeSpan timeSpan, string? reason = null)
    {
        if (timeSpan.Ticks <= 0)
            throw new Exception();

        lock (_lock)
        {
            var userLicenseData = InternalGetById(licenseId);

            var now = _dateTimeProvider.Now;
            if (userLicenseData.IsSuspended(_dateTimeProvider.Now))
                throw new PlayerLicenseAlreadySuspendedException(licenseId);

            userLicenseData.SuspendedUntil = now + timeSpan;
            userLicenseData.SuspendedReason = reason;
            _playerUserFeature.IncreaseVersion();
            Suspended?.Invoke(this, PlayerLicenseDto.Map(userLicenseData));
        }
    }

    public void UnSuspend(int licenseId)
    {
        lock (_lock)
        {
            var userLicenseData = InternalGetById(licenseId);
            if (!userLicenseData.IsSuspended(_dateTimeProvider.Now))
                throw new PlayerLicenseNotSuspendedException(licenseId);

            userLicenseData.SuspendedUntil = null;
            _playerUserFeature.IncreaseVersion();
            UnSuspended?.Invoke(this, PlayerLicenseDto.Map(userLicenseData));
        }
    }

    private UserLicenseData InternalGetById(int licenseId)
    {
        var userLicenseData = _licenses.Where(x => x.LicenseId == licenseId).FirstOrDefault();
        if (userLicenseData == null)
            throw new PlayerLicenseNotFoundException(licenseId);

        return userLicenseData;
    }

    private bool InternalHas(int licenseId, bool includeSuspended = false)
    {
        var query = _licenses.Where(x => x.LicenseId == licenseId);

        if (includeSuspended)
            query = query.Where(x => !x.IsSuspended(_dateTimeProvider.Now));

        lock (_lock)
            return query.Any();
    }

    public IEnumerator<PlayerLicenseDto> GetEnumerator()
    {
        lock (_lock)
            return new List<PlayerLicenseDto>(_licenses.Select(PlayerLicenseDto.Map)).AsReadOnly().GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
