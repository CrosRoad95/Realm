﻿namespace RealmCore.Server.Modules.Players.Licenses;

public sealed class PlayerLicensesFeature : IPlayerFeature, IEnumerable<PlayerLicenseDto>, IUsesUserPersistentData
{
    private readonly object _lock = new();
    private readonly IDateTimeProvider _dateTimeProvider;
    private ICollection<UserLicenseData> _licenses = [];

    public event Action<PlayerLicensesFeature, PlayerLicenseDto>? Added;
    public event Action<PlayerLicensesFeature, PlayerLicenseDto>? Suspended;
    public event Action<PlayerLicensesFeature, PlayerLicenseDto>? UnSuspended;
    public event Action? VersionIncreased;

    public RealmPlayer Player { get; init; }
    public PlayerLicensesFeature(PlayerContext playerContext, IDateTimeProvider dateTimeProvider)
    {
        Player = playerContext.Player;
        _dateTimeProvider = dateTimeProvider;
    }

    public void LogIn(UserData userData)
    {
        lock(_lock)
            _licenses = userData.Licenses;
    }

    public bool TryGetById(int licenseId, out PlayerLicenseDto playerLicenseDto)
    {
        lock (_lock)
        {
            var userLicenseData = InternalGetById(licenseId);
            if(userLicenseData != null)
            {
                playerLicenseDto = PlayerLicenseDto.Map(userLicenseData);
                return true;
            }
        }
        playerLicenseDto = null!;
        return false;
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
        UserLicenseData userLicenseData;
        lock (_lock)
        {
            if (InternalHas(licenseId, true))
                return false;

            userLicenseData = new UserLicenseData
            {
                LicenseId = licenseId,
            };

            _licenses.Add(userLicenseData);
        }

        VersionIncreased?.Invoke();
        Added?.Invoke(this, PlayerLicenseDto.Map(userLicenseData));
        return true;
    }

    public bool Has(int licenseId, bool includeSuspended = false)
    {
        lock(_lock)
            return InternalHas(licenseId, includeSuspended);
    }

    public void Suspend(int licenseId, TimeSpan timeSpan, string? reason = null)
    {
        if (timeSpan.Ticks <= 0)
            throw new Exception();

        UserLicenseData userLicenseData;
        lock (_lock)
        {
            userLicenseData = InternalGetById(licenseId);

            var now = _dateTimeProvider.Now;
            if (userLicenseData.IsSuspended(_dateTimeProvider.Now))
                throw new PlayerLicenseAlreadySuspendedException(licenseId);

            userLicenseData.SuspendedUntil = now + timeSpan;
            userLicenseData.SuspendedReason = reason;
        }

        VersionIncreased?.Invoke();
        Suspended?.Invoke(this, PlayerLicenseDto.Map(userLicenseData));
    }

    public void UnSuspend(int licenseId)
    {
        UserLicenseData userLicenseData;
        lock (_lock)
        {
            userLicenseData = InternalGetById(licenseId);
            if (!userLicenseData.IsSuspended(_dateTimeProvider.Now))
                throw new PlayerLicenseNotSuspendedException(licenseId);

            userLicenseData.SuspendedUntil = null;
        }

        VersionIncreased?.Invoke();
        UnSuspended?.Invoke(this, PlayerLicenseDto.Map(userLicenseData));
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

        return query.Any();
    }

    public IEnumerator<PlayerLicenseDto> GetEnumerator()
    {
        UserLicenseData[] view;
        {
            lock(_lock)
                view = [.. _licenses];
        }

        foreach (var settingData in view)
        {
            yield return PlayerLicenseDto.Map(settingData);
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
