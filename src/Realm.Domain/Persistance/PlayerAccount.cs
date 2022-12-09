using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Realm.Common.Exceptions;
using Realm.Configuration;
using Realm.Interfaces.Server.Services;
using Realm.Module.Scripting.Extensions;
using Realm.Persistance.Extensions;
using System.Security.Claims;

namespace Realm.Persistance.Scripting.Classes;

[NoDefaultScriptAccess]
public class PlayerAccount : IDisposable
{
    public const string ClaimDiscordUserIdName = "discord.user.id";

    private bool _disposed;
    private User _user;
    private readonly SignInManager<User> _signInManager;
    private readonly UserManager<User> _userManager;
    private readonly IDb _db;
    private readonly IAuthorizationService _authorizationService;
    private readonly IAccountsInUseService _accountsInUseService;
    private readonly IServiceProvider _serviceProvider;
    private readonly RealmConfigurationProvider _configurationProvider;
    private DateTime? _lastPlayTimeCounterStart;
    private DateTime? _loginDateTime;
    private ClaimsPrincipal? _claimsPrincipal;
    private string? _discordConnectionCode = null;
    private DateTime? _discordConnectionCodeValidUntil = null;
    private DiscordUser? _discord = null;

    public event Action<PlayerAccount>? NotifyNotSavedState;
    public event Action<PlayerAccount>? Disposed;

    [ScriptMember("id")]
    public string Id
    {
        get
        {
            CheckIfDisposed();
            return _user.Id.ToString().ToUpper();
        }
    }

    [ScriptMember("userName")]
    public string? UserName
    {
        get
        {
            CheckIfDisposed();
            return _user.UserName;
        }
    }

    [ScriptMember("registerDateTime")]
    public DateTime? RegisterDateTime
    {
        get
        {
            CheckIfDisposed();
            return _user.RegisteredDateTime;
        }
    }

    [ScriptMember("discord")]
    public DiscordUser? Discord
    {
        get
        {
            CheckIfDisposed();
            return _discord;
        }
    }

    public string? ComponentsData
    {
        get
        {
            CheckIfDisposed();
            return _user.Components;
        }
        set
        {
            CheckIfDisposed();
            _user.Components = value;
        }
    }

    public string? InventoryData
    {
        get
        {
            CheckIfDisposed();
            return _user.Inventory;
        }
        set
        {
            CheckIfDisposed();
            _user.Inventory = value;
        }
    }

    [ScriptMember("money")]
    public double Money
    {
        get => _user.Money; set
        {
            CheckIfDisposed();
            if (_user.Money == value)
                return;

            if (value < 0)
                throw new GameplayException("Unable to set money, money can not get negative");

            if (value > _configurationProvider.GetRequired<double>("Gameplay:MoneyLimit"))
                throw new GameplayException("Unable to set money, reached limit.");
            var moneyPrecision = _configurationProvider.GetRequired<int>("Gameplay:MoneyPrecision");
            var old = _user.Money;
            _user.Money = Math.Round(value, moneyPrecision);
            NotifyNotSavedState?.Invoke(this);
        }
    }

    [ScriptMember("playTime")]
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

    [ScriptMember("currentSessionPlayTime")]
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

    public PlayerAccount(SignInManager<User> signInManager, UserManager<User> userManager, IDb db, IAuthorizationService authorizationService,
        IAccountsInUseService accountsInUseService, IServiceProvider serviceProvider, RealmConfigurationProvider configurationProvider)
    {
        _user = null!;
        _signInManager = signInManager;
        _userManager = userManager;
        _db = db;
        _authorizationService = authorizationService;
        _accountsInUseService = accountsInUseService;
        _serviceProvider = serviceProvider;
        _configurationProvider = configurationProvider;
    }

    public async Task<string[]> InternalGetRoles()
    {
        CheckIfDisposed();

        return (await _userManager.GetRolesAsync(_user)).ToArray();
    }

    public async Task<Claim[]> InternalGetClaims()
    {
        CheckIfDisposed();

        return (await _userManager.GetClaimsAsync(_user)).ToArray();
    }

    [ScriptMember("giveMoney")]
    public bool GiveMoney(double amount)
    {
        CheckIfDisposed();

        if (_user.Money < 0)
            throw new GameplayException("Unable to set money, money can not get negative");

        if (amount > _configurationProvider.GetRequired<double>("Gameplay:MoneyLimit"))
            throw new GameplayException("Unable to set money, reached limit.");

        var moneyPrecision = _configurationProvider.GetRequired<int>("Gameplay:MoneyPrecision");
        Money = _user.Money + Math.Round(amount, moneyPrecision);
        return true;
    }

    public void SetUser(User user)
    {
        CheckIfDisposed();

        if (_user != null)
            throw new GameplayException("User already set.");
        _user = user;
    }

    [ScriptMember("isAuthenticated")]
    public bool IsAuthenticated
    {
        get
        {
            CheckIfDisposed();
            return _claimsPrincipal != null && _claimsPrincipal.Identity != null && _claimsPrincipal.Identity.IsAuthenticated;
        }
    }

    [ScriptMember("checkPasswordAsync")]
    public async Task<bool> CheckPasswordAsync(string password)
    {
        CheckIfDisposed();

        return await _userManager.CheckPasswordAsync(_user, password);
    }

    [ScriptMember("isInUse")]
    public bool IsInUse()
    {
        CheckIfDisposed();

        return _accountsInUseService.IsAccountIdInUse(Id);
    }

    public async Task SignIn(string? ip, string serial)
    {
        CheckIfDisposed();

        _loginDateTime = DateTime.Now;
        _lastPlayTimeCounterStart = DateTime.Now;
        if (_user.RegisterIp == null && ip != null)
            _user.RegisterIp = ip;
        _user.RegisterSerial ??= serial;

        if (ip != null)
            _user.LastIp = ip;
        _user.LastSerial = serial;
        await UpdateClaimsPrincipal();
        try
        {
            TryInitializeDiscordUser();
        }
        catch (Exception)
        {

        }
    }

    public async Task UpdateClaimsPrincipal()
    {
        _claimsPrincipal = await _signInManager.CreateUserPrincipalAsync(_user);
    }

    public void TryInitializeDiscordUser()
    {
        var claimValue = GetClaimValue(ClaimDiscordUserIdName);
        if (claimValue != null && ulong.TryParse(claimValue, out ulong discordUserId))
        {
            _discord = _serviceProvider.GetRequiredService<DiscordUser>();
            try
            {
                //_discord.InitializeById(discordUserId);
            }
            catch (Exception ex)
            {
                _discord = null;
                throw;
            }
        }
    }

    public async Task Save()
    {
        if (_lastPlayTimeCounterStart != null)
        {
            _user.PlayTime += (ulong)(DateTime.Now - _lastPlayTimeCounterStart.Value).Seconds;
            _lastPlayTimeCounterStart = DateTime.Now;
        }
        await _userManager.UpdateAsync(_user);
    }

    [ScriptMember("isInRole")]
    public bool IsInRole(string role)
    {
        CheckIfDisposed();

        if (!IsAuthenticated)
            return false;
        return _claimsPrincipal!.IsInRole(role);
    }

    [ScriptMember("hasClaim")]
    public bool HasClaim(string type)
    {
        CheckIfDisposed();

        if (!IsAuthenticated)
            return false;
        return _claimsPrincipal!.HasClaim(x => x.Type == type);
    }

    [ScriptMember("getClaimValue")]
    public string? GetClaimValue(string type)
    {
        CheckIfDisposed();

        if (!IsAuthenticated || !HasClaim(type))
            return null;

        return _claimsPrincipal!.Claims.First(x => x.Type == type).Value;
    }

    [ScriptMember("delete")]
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

    [ScriptMember("addClaim")]
    public async Task<bool> AddClaim(string type, string value)
    {
        CheckIfDisposed();

        var result = await _userManager.AddClaimAsync(_user, new Claim(type, value));
        if (result.Succeeded)
            await UpdateClaimsPrincipal();
        return result.Succeeded;
    }

    [ScriptMember("addClaims")]
    public async Task<bool> AddClaims(Dictionary<string, string> claims)
    {
        CheckIfDisposed();

        var result = await _userManager.AddClaimsAsync(_user, claims.Select(x => new Claim(x.Key, x.Value)));
        if (result.Succeeded)
            await UpdateClaimsPrincipal();
        return result.Succeeded;
    }

    [ScriptMember("addRole")]
    public async Task<bool> AddRole(string role)
    {
        CheckIfDisposed();

        var result = await _userManager.AddToRoleAsync(_user, role);
        if (result.Succeeded)
            await UpdateClaimsPrincipal();
        return result.Succeeded;
    }

    [ScriptMember("addRoles")]
    public async Task<bool> AddRoles(IEnumerable<string> role)
    {
        CheckIfDisposed();

        var result = await _userManager.AddToRolesAsync(_user, role);
        if (result.Succeeded)
            await UpdateClaimsPrincipal();
        return result.Succeeded;
    }

    [ScriptMember("getClaims")]
    public async Task<object> GetClaims()
    {
        CheckIfDisposed();

        return (await _userManager.GetClaimsAsync(_user)).Select(x => x.Type).ToArray().ToScriptArray();
    }

    [ScriptMember("getRoles")]
    public async Task<object> GetRoles()
    {
        CheckIfDisposed();

        return (await InternalGetRoles()).ToScriptArray();
    }

    [ScriptMember("removeClaim")]
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
            if (result.Succeeded)
                await UpdateClaimsPrincipal();
            return result.Succeeded;
        }
        return false;
    }

    [ScriptMember("removeRole")]
    public async Task<bool> RemoveRole(string role)
    {
        CheckIfDisposed();

        var result = await _userManager.RemoveFromRoleAsync(_user, role);
        if (result.Succeeded)
            await UpdateClaimsPrincipal();
        return result.Succeeded;
    }

    [ScriptMember("removeAllClaims")]
    public async Task<bool> RemoveAllClaims()
    {
        CheckIfDisposed();

        var claims = await _userManager.GetClaimsAsync(_user);
        var result = await _userManager.RemoveClaimsAsync(_user, claims);
        if (result.Succeeded)
            await UpdateClaimsPrincipal();
        return result.Succeeded;
    }

    [ScriptMember("hasData")]
    public async Task<bool> HasData(string key)
    {
        CheckIfDisposed();

        var playerData = await _db.UserData
            .AsNoTrackingWithIdentityResolution()
            .FirstOrDefaultAsync(x => x.UserId.ToString() == Id && x.Key == key);
        return playerData != null;
    }

    [ScriptMember("getData")]
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

    [ScriptMember("removeData")]
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

    [ScriptMember("setData")]
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

    [ScriptMember("getAllLicenses")]
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

    [ScriptMember("isLicenseSuspended")]
    public async Task<bool> IsLicenseSuspended(string licenseId)
    {
        CheckIfDisposed();

        var isSuspended = await _db.UserLicenses
            .AsNoTrackingWithIdentityResolution()
            .IsSuspended()
            .AnyAsync(x => x.UserId.ToString() == Id && x.LicenseId.ToLower() == licenseId.ToLower());

        return isSuspended;
    }

    [ScriptMember("getLastLicenseSuspensionReason")]
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

    [ScriptMember("addLicense")]
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

    [ScriptMember("hasLicense")]
    public async Task<bool> HasLicense(string licenseId, bool includeSuspended = false)
    {
        CheckIfDisposed();

        var query = _db.UserLicenses
            .AsNoTrackingWithIdentityResolution()
            .Where(x => x.UserId.ToString() == Id && x.LicenseId.ToLower() == licenseId.ToLower());

        if (!includeSuspended)
            query = query.NotSuspended();
        return await query.AnyAsync();
    }

    [ScriptMember("suspendLicense")]
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

    [ScriptMember("unSuspendLicense")]
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

    [ScriptMember("authorizePolicy")]
    public async Task<bool> AuthorizePolicy(string policy)
    {
        CheckIfDisposed();

        if (!IsAuthenticated)
            return false;

        var result = await _authorizationService.AuthorizeAsync(_claimsPrincipal!, policy);

        return result.Succeeded;
    }

    [ScriptMember("isConnectedWithDiscordAccount")]
    public bool IsConnectedWithDiscordAccount()
    {
        CheckIfDisposed();

        return Discord != null;
    }

    [ScriptMember("isDiscordConnectionCodeValid")]
    public bool IsDiscordConnectionCodeValid(string code)
    {
        CheckIfDisposed();

        if (!HasPendingDiscordConnectionCode())
            return false;

        return _discordConnectionCode == code;
    }

    [ScriptMember("hasPendingDiscordConnectionCode")]
    public bool HasPendingDiscordConnectionCode()
    {
        CheckIfDisposed();

        return _discordConnectionCodeValidUntil != null || _discordConnectionCodeValidUntil > DateTime.Now;
    }

    [ScriptMember("invalidateDiscordConnectionCode")]
    public void InvalidateDiscordConnectionCode()
    {
        CheckIfDisposed();

        _discordConnectionCode = null;
        _discordConnectionCodeValidUntil = null;
    }

    [ScriptMember("generateAndGetDiscordConnectionCode")]
    public string? GenerateAndGetDiscordConnectionCode(int validForMinutes = 2)
    {
        CheckIfDisposed();

        if (IsConnectedWithDiscordAccount())
            return null;

        if (validForMinutes <= 0)
            return null;
        _discordConnectionCode = Guid.NewGuid().ToString();
        _discordConnectionCodeValidUntil = DateTime.Now.AddMinutes(validForMinutes);
        return _discordConnectionCode;
    }

    public async Task SetDiscordUserId(ulong id)
    {
        CheckIfDisposed();

        if (HasClaim(ClaimDiscordUserIdName))
        {
            await RemoveClaim(ClaimDiscordUserIdName);
        }
        await AddClaim(ClaimDiscordUserIdName, id.ToString());
        try
        {
            TryInitializeDiscordUser();
        }
        catch (Exception)
        {
            await RemoveClaim(ClaimDiscordUserIdName);
            throw;
        }
    }

    private void CheckIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(GetType().FullName);
    }

    public override string ToString() => _user.ToString();

    public void Dispose()
    {
        Disposed?.Invoke(this);
        _disposed = true;
    }
}
