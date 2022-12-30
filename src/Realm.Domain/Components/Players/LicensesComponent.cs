namespace Realm.Domain.Components.Players;

public class LicensesComponent : Component
{
    private readonly List<UserLicense> _licenses = new();
    private readonly Guid _userId;

    public List<UserLicense> Licenses => _licenses;

    public LicensesComponent(Guid userId)
    {
        _userId = userId;
    }

    public LicensesComponent(IEnumerable<UserLicense> userLicenses, Guid userId)
    {
        _licenses = userLicenses.ToList();
        _userId = userId;
    }

    public UserLicense? GetLicense(string licenseId)
    {
        return _licenses.Where(x => x.LicenseId.ToLower() == licenseId.ToLower()).FirstOrDefault();
    }

    public IEnumerable<UserLicense> GetAllLicenses(bool includeSuspended = false)
    {
        return _licenses.ToArray();
    }

    public bool IsLicenseSuspended(string licenseId)
    {
        var isSuspended = _licenses.Where(x => x.LicenseId == licenseId && x.IsSuspended())
            .Any();

        return isSuspended;
    }

    public string? GetLastLicenseSuspensionReason(string licenseId)
    {
        var suspensionReason = _licenses
            .Where(x => x.LicenseId == licenseId)
            .Select(x => x.SuspendedReason)
            .FirstOrDefault();

        return suspensionReason;
    }

    public bool AddLicense(string licenseId)
    {
        if (HasLicense(licenseId, true))
            return false;
        var userLicense = new UserLicense
        {
            LicenseId = licenseId,
            UserId = _userId,
        };
        _licenses.Add(userLicense);
        return true;
    }

    public bool HasLicense(string licenseId, bool includeSuspended = false)
    {
        var query = _licenses
            .Where(x => x.LicenseId.ToLower() == licenseId.ToLower());

        if (includeSuspended)
            query = query.Where(x => x.IsSuspended());
        return query.Any();
    }

    public bool SuspendLicense(string licenseId, int timeInMinutes, string? reason = null)
    {
        if (timeInMinutes < 0)
            return false;

        var userLicense = GetLicense(licenseId);
        if (userLicense == null)
            return false;

        userLicense.SuspendedUntil = DateTime.Now.AddMinutes(timeInMinutes);
        userLicense.SuspendedReason = reason;
        return true;
    }

    public async Task<bool> UnSuspendLicense(string licenseId)
    {
        var userLicense = GetLicense(licenseId);
        if (userLicense == null)
            return false;

        userLicense.SuspendedUntil = null;
        return true;
    }
}
