namespace Realm.Persistance.Scripting.Classes;

public class PlayerAccount : IDisposable
{
    private bool _disposed;
    private User _user;
    private readonly SignInManager<User> _signInManager;
    private readonly UserManager<User> _userManager;
    private readonly IDb _db;
    private readonly IAuthorizationService _authorizationService;
    private readonly IAccountsInUseService _accountsInUseService;
    private readonly ILogger _logger;
    private DateTime? _lastPlayTimeCounterStart;
    private DateTime? _loginDateTime;
    private ClaimsPrincipal? _claimsPrincipal;

    public string Id
    {
        get
        {
            CheckIfDisposed();
            return _user.Id.ToString().ToUpper();
        }
    }

    public string UserName
    {
        get
        {
            CheckIfDisposed();
            return _user.UserName;
        }
    }

    public DateTime? RegisterDateTime
    {
        get
        {
            CheckIfDisposed();
            return _user.RegisteredDateTime;
        }
    }

    public PlayerAccount(SignInManager<User> signInManager, UserManager<User> userManager, IDb db, IAuthorizationService authorizationService,
        IAccountsInUseService accountsInUseService, ILogger logger)
    {
        _user = null!;
        _signInManager = signInManager;
        _userManager = userManager;
        _db = db;
        _authorizationService = authorizationService;
        _accountsInUseService = accountsInUseService;
        _logger = logger
            .ForContext<PlayerAccount>()
            .ForContext(new PlayerAccountEnricher(this));
    }

    public void SetUser(User user)
    {
        CheckIfDisposed();

        if (_user != null)
            throw new Exception("User already set.");
        _user = user;
    }

    public bool IsAuthenticated
    {
        get
        {
            CheckIfDisposed();
            return _claimsPrincipal != null && _claimsPrincipal.Identity != null && _claimsPrincipal.Identity.IsAuthenticated;
        }
    }

    public async Task<bool> CheckPasswordAsync(string password)
    {
        CheckIfDisposed();

        return await _userManager.CheckPasswordAsync(_user, password);
    }

    public bool IsInUse()
    {
        CheckIfDisposed();

        return _accountsInUseService.IsAccountIdInUse(Id);
    }

    public ulong PlayTime
    {
        get
        {
            CheckIfDisposed();
            if (_loginDateTime == null)
                return 0;
            return _user.PlayTime + (ulong)(DateTime.Now - _loginDateTime.Value).Seconds;
        }
    }
    
    public ulong CurrentSessionPlayTime
    {
        get
        {
            CheckIfDisposed();
            if (_loginDateTime == null)
                return 0;
            return (ulong)(DateTime.Now - _loginDateTime.Value).Seconds;
        }
    }

    [NoScriptAccess]
    public async Task SignIn(string? ip, string serial)
    {
        CheckIfDisposed();

        _loginDateTime = DateTime.Now;
        _lastPlayTimeCounterStart = DateTime.Now;
        if(_user.RegisterIp == null && ip != null)
            _user.RegisterIp = ip;
        if(_user.RegisterSerial == null)
            _user.RegisterSerial = serial;

        if(ip != null)
            _user.LastIp = ip;
        _user.LastSerial = serial;
        _claimsPrincipal = await _signInManager.CreateUserPrincipalAsync(_user);
    }

    [NoScriptAccess]
    public async Task Save()
    {
        CheckIfDisposed();

        if(_lastPlayTimeCounterStart != null)
        {
            _user.PlayTime += (ulong)(DateTime.Now - _lastPlayTimeCounterStart.Value).Seconds;
            _lastPlayTimeCounterStart = DateTime.Now;
        }
        await _userManager.UpdateAsync(_user);
        _logger.Verbose("Saved account");
    }

    public bool IsInRole(string role)
    {
        CheckIfDisposed();

        if (!IsAuthenticated)
            return false;
        return _claimsPrincipal!.IsInRole(role);
    }

    public bool HasClaim(string type)
    {
        CheckIfDisposed();

        if (!IsAuthenticated)
            return false;
        return _claimsPrincipal!.HasClaim(x => x.Type == type);
    }

    public string? GetClaimValue(string type)
    {
        CheckIfDisposed();

        if (!IsAuthenticated || !HasClaim(type))
            return null;

        return _claimsPrincipal!.Claims.First(x => x.Type == type).Value;
    }


    public async Task<bool> Delete()
    {
        CheckIfDisposed();

        var result = await _userManager.DeleteAsync(_user);
        if (result.Succeeded)
        {
            Dispose();
            return true;
        }
        return false;
    }

    public async Task<bool> AddClaim(string type, string value)
    {
        CheckIfDisposed();

        var result = await _userManager.AddClaimAsync(_user, new Claim(type, value));
        return result.Succeeded;
    }

    public async Task<bool> AddClaims(Dictionary<string, string> claims)
    {
        CheckIfDisposed();

        var result = await _userManager.AddClaimsAsync(_user, claims.Select(x => new Claim(x.Key, x.Value)));
        return result.Succeeded;
    }

    public async Task<bool> AddRole(string role)
    {
        CheckIfDisposed();

        var result = await _userManager.AddToRoleAsync(_user, role);
        return result.Succeeded;
    }

    public async Task<bool> AddRoles(IEnumerable<string> role)
    {
        CheckIfDisposed();

        var result = await _userManager.AddToRolesAsync(_user, role);
        return result.Succeeded;
    }

    public async Task<object> GetClaims()
    {
        CheckIfDisposed();

        return (await _userManager.GetClaimsAsync(_user)).Select(x => x.Type).ToArray().ToScriptArray();
    }

    [NoScriptAccess]
    public async Task<string[]> InternalGetRoles()
    {
        CheckIfDisposed();

        return (await _userManager.GetRolesAsync(_user)).ToArray();
    }


    [NoScriptAccess]
    public async Task<Claim[]> InternalGetClaims()
    {
        CheckIfDisposed();

        return (await _userManager.GetClaimsAsync(_user)).ToArray();
    }

    public async Task<object> GetRoles()
    {
        CheckIfDisposed();

        return (await InternalGetRoles()).ToScriptArray();
    }

    public async Task<bool> RemoveClaim(string type, string? value = null)
    {
        CheckIfDisposed();

        var claims = await _userManager.GetClaimsAsync(_user);
        Claim? claim;
        if (value != null)
            claim = claims.FirstOrDefault(x => x.Type == type && x.Value == value);
        else
            claim = claims.FirstOrDefault(x => x.Type == type);

        if (claim != null)
        {
            var result = await _userManager.RemoveClaimAsync(_user, claim);
            return result.Succeeded;
        }
        return false;
    }

    public async Task<bool> RemoveRole(string role)
    {
        CheckIfDisposed();

        var result = await _userManager.RemoveFromRoleAsync(_user, role);
        return result.Succeeded;
    }

    public async Task<bool> RemoveAllClaims()
    {
        CheckIfDisposed();

        var claims = await _userManager.GetClaimsAsync(_user);
        var result = await _userManager.RemoveClaimsAsync(_user, claims);
        return result.Succeeded;
    }

    public async Task<bool> HasData(string key)
    {
        CheckIfDisposed();

        var playerData = await _db.UserData
            .AsNoTrackingWithIdentityResolution()
            .FirstOrDefaultAsync(x => x.UserId.ToString() == Id && x.Key == key);
        return playerData != null;
    }

    public async Task<string?> GetData(string key)
    {
        CheckIfDisposed();

        var playerData = await _db.UserData
            .AsNoTrackingWithIdentityResolution()
            .FirstOrDefaultAsync(x => x.UserId.ToString() == Id && x.Key == key);
        if (playerData == null)
            return null;
        return playerData.Value;
    }

    public async Task<bool> RemoveData(string key)
    {
        CheckIfDisposed();

        var playerData = await _db.UserData.FirstOrDefaultAsync(x => x.UserId.ToString() == Id && x.Key == key);
        if (playerData == null)
            return false;

        _db.UserData.Remove(playerData);
        var savedEntities = await _db.SaveChangesAsync();
        return savedEntities == 1;
    }

    public async Task<bool> SetData(string key, string value)
    {
        CheckIfDisposed();

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
        CheckIfDisposed();

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
        CheckIfDisposed();

        var isSuspended = await _db.UserLicenses
            .AsNoTrackingWithIdentityResolution()
            .IsSuspended()
            .AnyAsync(x => x.UserId.ToString() == Id && x.LicenseId.ToLower() == licenseId.ToLower());

        return isSuspended;
    }
    public async Task<string?> GetLastLicenseSuspensionReason()
    {
        CheckIfDisposed();

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
        CheckIfDisposed();

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
        CheckIfDisposed();

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
        CheckIfDisposed();

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
        CheckIfDisposed();

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


    public async Task<bool> AuthorizePolicy(string policy)
    {
        CheckIfDisposed();

        if (!IsAuthenticated)
            return false;

        var result = await _authorizationService.AuthorizeAsync(_claimsPrincipal!, policy);

        return result.Succeeded;
    }

    [NoScriptAccess]
    private void CheckIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(GetType().FullName);
    }

    public string LongUserFriendlyName() => ToString();
    public override string ToString() => _user.ToString();


    [NoScriptAccess]
    public void Dispose()
    {
        _disposed = true;
    }
}
