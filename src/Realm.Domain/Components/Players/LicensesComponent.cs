using Microsoft.EntityFrameworkCore;
using Realm.Persistance;

namespace Realm.Domain.Components.Players;

public class LicensesComponent : Component
{
    private List<UserLicense> _userLicenses = new();
    private string? userId;

    public List<UserLicense> UserLicenses { get => _userLicenses; set => _userLicenses = value; }

    public LicensesComponent(List<UserLicense> userLicenses)
    {
        UserLicenses = userLicenses;    
    }

    //public override async Task Load()
    //{
    //    var id = Entity.InternalGetRequiredComponent<AccountComponent>().Id;
    //    var db = Entity.GetRequiredService<IDb>();

    //    userId = id;
    //    _userLicenses = await db.UserLicenses
    //        .Where(x => x.UserId.ToString() == id)
    //        .ToListAsync();
    //}

    public UserLicense? GetLicense(string licenseId)
    {
        return _userLicenses.Where(x => x.LicenseId.ToLower() == licenseId.ToLower()).FirstOrDefault();
    }

    public IEnumerable<UserLicense> GetAllLicenses(bool includeSuspended = false)
    {
        return _userLicenses.ToArray();
    }

    public bool IsLicenseSuspended(string licenseId)
    {
        var isSuspended = _userLicenses.Where(x => x.LicenseId == licenseId && x.IsSuspended())
            .Any();

        return isSuspended;
    }

    public string? GetLastLicenseSuspensionReason(string licenseId)
    {
        var suspensionReason = _userLicenses
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
            UserId = Guid.Parse(userId),
        };
        _userLicenses.Add(userLicense);
        return true;
    }

    public bool HasLicense(string licenseId, bool includeSuspended = false)
    {
        var query = _userLicenses
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
