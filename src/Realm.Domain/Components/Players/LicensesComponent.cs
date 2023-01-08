using License = Realm.Domain.Concepts.License;

namespace Realm.Domain.Components.Players;

public class LicensesComponent : Component
{
    private readonly List<License> _licenses = new();

    public IEnumerable<License> Licenses => _licenses;

    public LicensesComponent()
    {
    }

    public LicensesComponent(IEnumerable<UserLicense> userLicenses)
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
        return _licenses.Where(x => x.licenseId.ToLower() == licenseId.ToLower()).FirstOrDefault();
    }

    public bool IsLicenseSuspended(string licenseId)
    {
        var isSuspended = _licenses.Where(x => x.licenseId == licenseId && x.IsSuspended)
            .Any();

        return isSuspended;
    }

    public string? GetLastLicenseSuspensionReason(string licenseId)
    {
        var suspensionReason = _licenses
            .Where(x => x.licenseId == licenseId)
            .Select(x => x.suspendedReason)
            .FirstOrDefault();

        return suspensionReason;
    }

    public bool AddLicense(string licenseId)
    {
        if (HasLicense(licenseId, true))
            return false;
        var userLicense = new License
        {
            licenseId = licenseId,
        };
        _licenses.Add(userLicense);
        return true;
    }

    public bool HasLicense(string licenseId, bool includeSuspended = false)
    {
        var query = _licenses.Where(x => x.licenseId == licenseId);

        if (includeSuspended)
            query = query.Where(x => !x.IsSuspended);
        return query.Any();
    }

    public bool SuspendLicense(string licenseId, TimeSpan timeSpan, string? reason = null)
    {
        if (timeSpan.Ticks <= 0)
            throw new Exception();

        var index = _licenses.FindIndex(x => x.licenseId == licenseId);
        if (index == -1)
            throw new Exception();

        var previous = _licenses[index];
        previous.suspendedUntil = DateTime.Now + timeSpan;
        previous.suspendedReason = reason;
        _licenses[index] = previous;
        return true;
    }

    public async Task<bool> UnSuspendLicense(string licenseId)
    {
        var index = _licenses.FindIndex(x => x.licenseId == licenseId);
        if (index == -1)
            throw new Exception();

        var previous = _licenses[index];
        previous.suspendedUntil = null;
        _licenses[index] = previous;
        return true;
    }
}
