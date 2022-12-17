using Microsoft.EntityFrameworkCore;
using Realm.Module.Scripting.Extensions;
using Realm.Persistance;
using Realm.Resources.LuaInterop;

namespace Realm.Domain.New;

public class LicensesComponent : Component
{
    private List<UserLicense> _userLicenses = new();
    private string? userId;
    public LicensesComponent()
    {

    }

    public override async Task Load()
    {
        var id = Entity.InternalGetRequiredComponent<AccountComponent>().Id;
        var db = Entity.GetRequiredService<IDb>();

        userId = id;
        _userLicenses = await db.UserLicenses
            .Where(x => x.UserId.ToString() == id)
            .ToListAsync();
    }

    public UserLicense? GetLicense(string licenseId)
    {
        return _userLicenses.Where(x => x.LicenseId.ToLower() == licenseId.ToLower()).FirstOrDefault();
    }

    [ScriptMember("getAllLicenses")]
    public object GetAllLicenses(bool includeSuspended = false)
    {
        return _userLicenses.ToArray().ToScriptArray();
    }

    [ScriptMember("isLicenseSuspended")]
    public bool IsLicenseSuspended(string licenseId)
    {
        var isSuspended = _userLicenses.Where(x => x.LicenseId == licenseId && x.IsSuspended())
            .Any();

        return isSuspended;
    }

    [ScriptMember("getLastLicenseSuspensionReason")]
    public string? GetLastLicenseSuspensionReason(string licenseId)
    {
        var suspensionReason = _userLicenses
            .Where(x => x.LicenseId == licenseId)
            .Select(x => x.SuspendedReason)
            .FirstOrDefault();

        return suspensionReason;
    }

    [ScriptMember("addLicense")]
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

    [ScriptMember("hasLicense")]
    public bool HasLicense(string licenseId, bool includeSuspended = false)
    {
        var query = _userLicenses
            .Where(x => x.LicenseId.ToLower() == licenseId.ToLower());

        if (includeSuspended)
            query = query.Where(x => x.IsSuspended());
        return query.Any();
    }

    [ScriptMember("suspendLicense")]
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

    [ScriptMember("unSuspendLicense")]
    public async Task<bool> UnSuspendLicense(string licenseId)
    {
        var userLicense = GetLicense(licenseId);
        if (userLicense == null)
            return false;

        userLicense.SuspendedUntil = null;
        return true;
    }

}
