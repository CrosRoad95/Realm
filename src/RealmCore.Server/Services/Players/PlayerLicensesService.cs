﻿using System.Collections;

namespace RealmCore.Server.Services.Players;

internal class PlayerLicensesService : IPlayerLicensesService
{
    private readonly object _lock = new();
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IPlayerUserService _playerUserService;
    private ICollection<UserLicenseData> _licenses = [];

    public event Action<IPlayerLicensesService, int>? Added;
    public event Action<IPlayerLicensesService, int, DateTime, string?>? Suspended;
    public event Action<IPlayerLicensesService, int>? UnSuspended;
    public RealmPlayer Player { get; private set; }
    public PlayerLicensesService(PlayerContext playerContext, IDateTimeProvider dateTimeProvider, IPlayerUserService playerUserService)
    {
        Player = playerContext.Player;
        _dateTimeProvider = dateTimeProvider;
        _playerUserService = playerUserService;
        playerUserService.SignedIn += HandleSignedIn;
        playerUserService.SignedOut += HandleSignedOut;
    }

    private void HandleSignedIn(IPlayerUserService playerUserService, RealmPlayer _)
    {
        lock(_lock)
            _licenses = playerUserService.User.Licenses;
    }

    private void HandleSignedOut(IPlayerUserService playerUserService, RealmPlayer _)
    {
        lock (_lock)
            _licenses = [];
    }

    public LicenseDTO? Get(int licenseId)
    {
        lock (_lock)
            return LicenseDTO.Map(_licenses.Where(x => x.LicenseId == licenseId).FirstOrDefault());
    }

    public bool IsSuspended(int licenseId)
    {
        lock (_lock)
            return _licenses
                .Where(x => x.LicenseId == licenseId && x.IsSuspended(_dateTimeProvider.Now))
                .Any();
    }

    public bool TryAdd(int licenseId)
    {
        var userLicense = new UserLicenseData
        {
            LicenseId = licenseId,
        };

        lock (_lock)
        {
            if (InternalHas(licenseId, true))
                return false;

            _licenses.Add(userLicense);
            _playerUserService.IncreaseVersion();
            Added?.Invoke(this, licenseId);
            return true;
        }
    }

    private bool InternalHas(int licenseId, bool includeSuspended = false)
    {
        var query = _licenses.Where(x => x.LicenseId == licenseId);

        if (includeSuspended)
            query = query.Where(x => !(x.SuspendedUntil != null && x.SuspendedUntil > _dateTimeProvider.Now));

        lock (_lock)
            return query.Any();
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
            var license = _licenses.Where(x => x.LicenseId == licenseId).FirstOrDefault();
            if (license == null)
                throw new Exception();

            license.SuspendedUntil = _dateTimeProvider.Now + timeSpan;
            license.SuspendedReason = reason;
            _playerUserService.IncreaseVersion();
            Suspended?.Invoke(this, licenseId, license.SuspendedUntil.Value, license.SuspendedReason);
        }
    }

    public void UnSuspend(int licenseId)
    {
        lock (_lock)
        {
            var license = _licenses.Where(x => x.LicenseId == licenseId).FirstOrDefault();
            if (license == null)
                throw new Exception();

            license.SuspendedUntil = null;
            _playerUserService.IncreaseVersion();
            UnSuspended?.Invoke(this, licenseId);
        }
    }

    public IEnumerator<LicenseDTO> GetEnumerator()
    {
        lock (_lock)
            return new List<LicenseDTO>(_licenses.Select(LicenseDTO.Map)).AsReadOnly().GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}