using RealmCore.Persistence.Data;

namespace RealmCore.Server.Components.Players;

[ComponentUsage(false)]
public class LicensesComponent : Component
{
    [Inject]
    private IDateTimeProvider DateTimeProvider { get; set; } = default!;

    private readonly List<License> _licenses = new();

    public IReadOnlyList<License> Licenses
    {
        get
        {
            lock (_lock)
                return new List<License>(_licenses).AsReadOnly();
        }
    }
    private readonly object _lock = new();

    public event Action<LicensesComponent, int>? LicenseAdded;
    public event Action<LicensesComponent, int, DateTime, string?>? LicenseSuspended;
    public event Action<LicensesComponent, int>? LicenseUnSuspended;
    public LicensesComponent()
    {
    }

    internal LicensesComponent(IEnumerable<UserLicenseData> userLicenses)
    {
        _licenses = userLicenses.Select(x => new License
        {
            licenseId = x.LicenseId,
            suspendedReason = x.SuspendedReason,
            suspendedUntil = x.SuspendedUntil,
        }).ToList();
    }

    public License? GetLicense(int licenseId)
    {
        ThrowIfDisposed();

        lock (_lock)
            return _licenses.Where(x => x.licenseId == licenseId).FirstOrDefault();
    }

    public bool IsLicenseSuspended(int licenseId)
    {
        ThrowIfDisposed();

        lock (_lock)
            return _licenses
                .Where(x => x.licenseId == licenseId && x.IsSuspended(DateTimeProvider))
                .Any();
    }

    public string? GetLastLicenseSuspensionReason(int licenseId)
    {
        ThrowIfDisposed();

        lock (_lock)
            return _licenses
                .Where(x => x.licenseId == licenseId)
                .Select(x => x.suspendedReason)
                .FirstOrDefault();
    }

    public bool TryAddLicense(int licenseId)
    {
        ThrowIfDisposed();

        var userLicense = new License
        {
            licenseId = licenseId,
        };

        lock (_lock)
        {
            if (InternalHasLicense(licenseId, true))
                return false;

            _licenses.Add(userLicense);
            LicenseAdded?.Invoke(this, licenseId);
            return true;
        }
    }

    private bool InternalHasLicense(int licenseId, bool includeSuspended = false)
    {
        var query = _licenses.Where(x => x.licenseId == licenseId);

        if (includeSuspended)
            query = query.Where(x => !(x.suspendedUntil != null && x.suspendedUntil > DateTimeProvider.Now));

        lock (_lock)
            return query.Any();
    }

    public bool HasLicense(int licenseId, bool includeSuspended = false)
    {
        ThrowIfDisposed();

        lock (_lock)
            return InternalHasLicense(licenseId, includeSuspended);
    }

    public void SuspendLicense(int licenseId, TimeSpan timeSpan, string? reason = null)
    {
        ThrowIfDisposed();

        if (timeSpan.Ticks <= 0)
            throw new Exception();

        lock (_lock)
        {
            var index = _licenses.FindIndex(x => x.licenseId == licenseId);
            if (index == -1)
                throw new Exception();

            var previous = _licenses[index];
            previous.suspendedUntil = DateTimeProvider.Now + timeSpan;
            previous.suspendedReason = reason;
            _licenses[index] = previous;
            LicenseSuspended?.Invoke(this, licenseId, previous.suspendedUntil.Value, previous.suspendedReason);
        }
    }

    public void UnSuspendLicense(int licenseId)
    {
        ThrowIfDisposed();

        lock (_lock)
        {
            var index = _licenses.FindIndex(x => x.licenseId == licenseId);
            if (index == -1)
                throw new Exception();

            var license = _licenses[index];
            license.suspendedUntil = null;
            _licenses[index] = license;
            LicenseUnSuspended?.Invoke(this, licenseId);
        }
    }
}
