using License = Realm.Domain.Concepts.License;

namespace Realm.Domain.Components.Players;

public class LicensesComponent : Component
{
    private readonly List<License> _licenses = new();

    public IReadOnlyList<License> Licenses => _licenses;
    private readonly object _licensesLock = new object();

    public LicensesComponent()
    {
    }

    internal LicensesComponent(IEnumerable<UserLicense> userLicenses)
    {
        _licenses = userLicenses.Select(x => new License
        {
            licenseId = x.LicenseId,
            suspendedReason = x.SuspendedReason,
            suspendedUntil = x.SuspendedUntil,
        }).ToList();
    }

    public License? GetLicense(string licenseId)
    {
        ThrowIfDisposed();

        lock(_licensesLock)
            return _licenses.Where(x => x.licenseId.ToLower() == licenseId.ToLower()).FirstOrDefault();
    }

    public bool IsLicenseSuspended(string licenseId)
    {
        lock(_licensesLock)
            return _licenses.Where(x => x.licenseId == licenseId && x.IsSuspended)
            .Any();
    }

    public string? GetLastLicenseSuspensionReason(string licenseId)
    {
        lock(_licensesLock)
        return _licenses
            .Where(x => x.licenseId == licenseId)
            .Select(x => x.suspendedReason)
            .FirstOrDefault();
    }

    public bool AddLicense(string licenseId)
    {
        var userLicense = new License
        {
            licenseId = licenseId,
        };

        lock (_licensesLock)
        {
            if (InternalHasLicense(licenseId, true))
                return false;

            _licenses.Add(userLicense);
            return true;
        }
    }

    private bool InternalHasLicense(string licenseId, bool includeSuspended = false)
    {
        var query = _licenses.Where(x => x.licenseId == licenseId);

        if (includeSuspended)
            query = query.Where(x => !x.IsSuspended);
        return query.Any();
    }
    
    public bool HasLicense(string licenseId, bool includeSuspended = false)
    {
        lock (_licensesLock)
            return InternalHasLicense(licenseId, includeSuspended);
    }

    public void SuspendLicense(string licenseId, TimeSpan timeSpan, string? reason = null)
    {
        if (timeSpan.Ticks <= 0)
            throw new Exception();

        lock (_licensesLock)
        {
            var index = _licenses.FindIndex(x => x.licenseId == licenseId);
            if (index == -1)
                throw new Exception();

            var previous = _licenses[index];
            previous.suspendedUntil = DateTime.Now + timeSpan;
            previous.suspendedReason = reason;
            _licenses[index] = previous;
        }
    }

    public void UnSuspendLicense(string licenseId)
    {
        lock (_licensesLock)
        {
            var index = _licenses.FindIndex(x => x.licenseId == licenseId);
            if (index == -1)
                throw new Exception();

            var previous = _licenses[index];
            previous.suspendedUntil = null;
            _licenses[index] = previous;
        }
    }
}
