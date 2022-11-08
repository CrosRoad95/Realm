using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json.Linq;
using Realm.Persistance.Extensions;

namespace Realm.Persistance.Scripting.Classes;

public class PlayerAccount : IDisposable
{
    private readonly User _user;
    private readonly UserManager<User> _userManager;
    private readonly IDb _db;
    private bool _disposed;

    [NoScriptAccess]
    public User User => _user;

    public string Id => _user.Id.ToString().ToUpper();
    public string UserName => _user.UserName;
    public DateTime? RegisterDateTime => _user.RegisteredDateTime;
    public PlayerAccount(User user, UserManager<User> userManager, IDb db)
    {
        _user = user;
        _userManager = userManager;
        _db = db;
    }

    public override string ToString() => _user.ToString();

    public async Task<bool> Delete()
    {
        if (_disposed)
            throw new ObjectDisposedException(GetType().ShortDisplayName());

        var result = await _userManager.DeleteAsync(_user);
        if(result.Succeeded)
        {
            Dispose();
            return true;
        }
        return false;
    }

    public async Task<bool> AddClaim(string type, string value)
    {
        if (_disposed)
            throw new ObjectDisposedException(GetType().ShortDisplayName());

        var result = await _userManager.AddClaimAsync(_user, new Claim(type, value));
        return result.Succeeded;
    }

    public async Task<bool> AddClaims(Dictionary<string, string> claims)
    {
        if (_disposed)
            throw new ObjectDisposedException(GetType().ShortDisplayName());

        var result = await _userManager.AddClaimsAsync(_user, claims.Select(x => new Claim(x.Key, x.Value)));
        return result.Succeeded;
    }

    public async Task<bool> AddRole(string role)
    {
        if (_disposed)
            throw new ObjectDisposedException(GetType().ShortDisplayName());

        var result = await _userManager.AddToRoleAsync(_user, role);
        return result.Succeeded;
    }
    
    public async Task<bool> AddRoles(IEnumerable<string> role)
    {
        if (_disposed)
            throw new ObjectDisposedException(GetType().ShortDisplayName());

        var result = await _userManager.AddToRolesAsync(_user, role);
        return result.Succeeded;
    }

    public async Task<object> GetClaims()
    {
        if (_disposed)
            throw new ObjectDisposedException(GetType().ShortDisplayName());

        return (await _userManager.GetClaimsAsync(_user)).Select(x => x.Type).ToArray().ToScriptArray();
    }

    [NoScriptAccess]
    public async Task<string[]> InternalGetRoles()
    {
        if (_disposed)
            throw new ObjectDisposedException(GetType().ShortDisplayName());

        return (await _userManager.GetRolesAsync(_user)).ToArray();
    }
    

    [NoScriptAccess]
    public async Task<Claim[]> InternalGetClaims()
    {
        if (_disposed)
            throw new ObjectDisposedException(GetType().ShortDisplayName());

        return (await _userManager.GetClaimsAsync(_user)).ToArray();
    }

    public async Task<object> GetRoles()
    {
        if (_disposed)
            throw new ObjectDisposedException(GetType().ShortDisplayName());

        return (await InternalGetRoles()).ToScriptArray();
    }

    public async Task<bool> RemoveClaim(string type, string? value = null)
    {
        if (_disposed)
            throw new ObjectDisposedException(GetType().ShortDisplayName());
        var claims = await _userManager.GetClaimsAsync(_user);
        Claim? claim;
        if (value != null)
            claim = claims.FirstOrDefault(x => x.Type == type && x.Value == value);
        else
            claim = claims.FirstOrDefault(x => x.Type == type);

        if(claim != null)
        {
            var result = await _userManager.RemoveClaimAsync(_user, claim);
            return result.Succeeded;
        }
        return false;
    }

    public async Task<bool> RemoveRole(string role)
    {
        if (_disposed)
            throw new ObjectDisposedException(GetType().ShortDisplayName());

        var result = await _userManager.RemoveFromRoleAsync(_user, role);
        return result.Succeeded;
    }

    public async Task<bool> RemoveAllClaims()
    {
        var claims = await _userManager.GetClaimsAsync(_user);
        var result = await _userManager.RemoveClaimsAsync(_user, claims);
        return result.Succeeded;
    }

    public async Task<bool> HasData(string key)
    {
        var playerData = await _db.UserData
            .AsNoTrackingWithIdentityResolution()
            .FirstOrDefaultAsync(x => x.UserId.ToString() == Id && x.Key == key);
        return playerData != null;
    }

    public async Task<string?> GetData(string key)
    {
        var playerData = await _db.UserData
            .AsNoTrackingWithIdentityResolution()
            .FirstOrDefaultAsync(x => x.UserId.ToString() == Id && x.Key == key);
        if (playerData == null)
            return null;
        return playerData.Value;
    }

    public async Task<bool> RemoveData(string key)
    {
        var playerData = await _db.UserData.FirstOrDefaultAsync(x => x.UserId.ToString() == Id && x.Key == key);
        if (playerData == null)
            return false;

        _db.UserData.Remove(playerData);
        var savedEntities = await _db.SaveChangesAsync();
        return savedEntities == 1;
    }

    public async Task<bool> SetData(string key, string value)
    {
        int savedEntities;

        var playerData = await _db.UserData.FirstOrDefaultAsync(x => x.UserId.ToString() == Id && x.Key == key);
        if (playerData == null)
        {
            playerData = new UserData
            {
                Key = key,
                UserId = Guid.Parse(Id),
                Value = value
            };
            _db.UserData.Add(playerData);
            savedEntities = await _db.SaveChangesAsync();
            return savedEntities == 1;
        }
        if (playerData.Value == value)
            return true;

        playerData.Value = value;
        _db.UserData.Update(playerData);
        savedEntities = await _db.SaveChangesAsync();
        return savedEntities == 1;
    }

    public async Task<object> GetAllLicenses(bool includeSuspended = false)
    {
        var query = _db.UserLicenses
            .AsNoTrackingWithIdentityResolution()
            .Where(x => x.UserId.ToString() == Id);
        if (includeSuspended)
            query = query.IsSuspended();
        else
            query = query.NotSuspended();
        var licenses = await query.Select(x => x.LicenseId).ToListAsync();
        return licenses.ToArray().ToScriptArray();
    }
    
    public async Task<bool> IsLicenseSuspended(string licenseId)
    {
        var isSuspended = await _db.UserLicenses
            .AsNoTrackingWithIdentityResolution()
            .IsSuspended()
            .AnyAsync(x => x.UserId.ToString() == Id && x.LicenseId.ToLower() == licenseId.ToLower());

        return isSuspended;
    }
    public async Task<string?> GetLastLicenseSuspensionReason()
    {
        var suspensionReason = await _db.UserLicenses
            .AsNoTrackingWithIdentityResolution()
            .Where(x => x.UserId.ToString() == Id)
            .IsSuspended()
            .Select(x => x.SuspendedReason)
            .FirstOrDefaultAsync();

        return suspensionReason;
    }

    public async Task<bool> AddLicense(string licenseId)
    {
        var userLicense = await _db.UserLicenses.FirstOrDefaultAsync(x => x.UserId.ToString() == Id && x.LicenseId.ToLower() == licenseId.ToLower());
        if (userLicense != null)
            return false;
        userLicense = new UserLicense
        {
            LicenseId = licenseId,
            UserId = Guid.Parse(Id),
        };
        _db.UserLicenses.Add(userLicense);
        var savedEntities = await _db.SaveChangesAsync();
        return savedEntities == 1;
    }

    public async Task<bool> HasLicense(string licenseId, bool includeSuspended = false)
    {
        var query = _db.UserLicenses
            .AsNoTrackingWithIdentityResolution()
            .Where(x => x.UserId.ToString() == Id && x.LicenseId.ToLower() == licenseId.ToLower());

        if (!includeSuspended)
            query = query.NotSuspended();
        var all = await query.ToListAsync();
        return await query.AnyAsync();
    }

    public async Task<bool> SuspendLicense(string licenseId, int timeInMinutes, string? reason = null)
    {
        if (timeInMinutes < 0)
            return false;

        var userLicense = await _db.UserLicenses
            .FirstOrDefaultAsync(x => x.UserId.ToString() == Id && x.LicenseId.ToLower() == licenseId.ToLower());
        if (userLicense == null)
            return false;

        userLicense.SuspendedUntil = DateTime.Now.AddMinutes(timeInMinutes);
        userLicense.SuspendedReason = reason;
        _db.UserLicenses.Update(userLicense);
        var savedEntities = await _db.SaveChangesAsync();
        return savedEntities == 1;
    }

    public async Task<bool> UnSuspendLicense(string licenseId)
    {
        var userLicense = await _db.UserLicenses
            .IsSuspended()
            .FirstOrDefaultAsync(x => x.UserId.ToString() == Id && x.LicenseId.ToLower() == licenseId.ToLower());
        if (userLicense == null)
            return false;

        userLicense.SuspendedUntil = null;
        _db.UserLicenses.Update(userLicense);
        var savedEntities = await _db.SaveChangesAsync();
        return savedEntities == 1;
    }

    [NoScriptAccess]
    public void Dispose()
    {
        _disposed = true;
    }
}
