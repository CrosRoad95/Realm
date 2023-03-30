using System.Collections.Concurrent;

namespace Realm.Domain.Components.Players;

[ComponentUsage(false)]
public class AccountComponent : AsyncComponent
{
    [Inject]
    private UserManager<User> UserManager { get; set; } = default!;
    [Inject]
    private SignInManager<User> SignInManager { get; set; } = default!;

    private readonly User _user;
    private ClaimsPrincipal _claimsPrincipal = default!;
    private List<int> _upgrades = new();
    private object _upgradesLock = new();
    private readonly ConcurrentDictionary<int, string> _settings = new();

    internal User User => _user;
    internal ClaimsPrincipal ClaimsPrincipal => _claimsPrincipal ?? throw new ArgumentNullException(nameof(_claimsPrincipal));
    public int Id => _user.Id;
    public string? UserName => _user.UserName;
    public IReadOnlyList<int> Upgrades => _upgrades;
    public ICollection<int> Settings => _settings.Keys;

    public event Action<AccountComponent, int>? UpgradeAdded;
    public event Action<AccountComponent, int>? UpgradeRemoved;
    public event Action<AccountComponent, ClaimsPrincipal>? ClaimsPrincipalUpdated;

    internal AccountComponent(User user)
    {
        _user = user;
        _upgrades = _user.Upgrades.Select(x => x.UpgradeId).ToList();
        foreach (var item in _user.Settings)
        {
            _settings[item.SettingId] = item.Value;
        }
    }
    protected override async Task LoadAsync()
    {
        await UpdateClaimsPrincipal();
    }

    private async Task UpdateClaimsPrincipal()
    {
        _claimsPrincipal = await SignInManager.CreateUserPrincipalAsync(_user);
        ClaimsPrincipalUpdated?.Invoke(this, _claimsPrincipal);
    }

    public bool IsInRole(string role)
    {
        ThrowIfDisposed();

        return _claimsPrincipal!.IsInRole(role);
    }

    public bool HasClaim(string type)
    {
        ThrowIfDisposed();

        return _claimsPrincipal!.HasClaim(x => x.Type == type);
    }

    public string? GetClaimValue(string type)
    {
        ThrowIfDisposed();

        return _claimsPrincipal!.Claims.First(x => x.Type == type).Value;
    }

    public async Task<bool> AddClaim(string type, string value)
    {
        ThrowIfDisposed();

        var result = await UserManager.AddClaimAsync(_user, new Claim(type, value));
        if (result.Succeeded)
            await UpdateClaimsPrincipal();
        return result.Succeeded;
    }

    public async Task<bool> AddClaims(Dictionary<string, string> claims)
    {
        ThrowIfDisposed();

        var result = await UserManager.AddClaimsAsync(_user, claims.Select(x => new Claim(x.Key, x.Value)));
        if (result.Succeeded)
            await UpdateClaimsPrincipal();
        return result.Succeeded;
    }

    public async Task<bool> AddRole(string role)
    {
        ThrowIfDisposed();

        var result = await UserManager.AddToRoleAsync(_user, role);
        if (result.Succeeded)
            await UpdateClaimsPrincipal();
        return result.Succeeded;
    }

    public async Task<bool> AddRoles(IEnumerable<string> role)
    {
        ThrowIfDisposed();

        var result = await UserManager.AddToRolesAsync(_user, role);
        if (result.Succeeded)
            await UpdateClaimsPrincipal();
        return result.Succeeded;
    }

    public async Task<IEnumerable<string>> GetClaims()
    {
        ThrowIfDisposed();

        return (await UserManager.GetClaimsAsync(_user)).Select(x => x.Type).ToList();
    }

    public async Task<IEnumerable<string>> GetRoles()
    {
        return (await UserManager.GetRolesAsync(_user)).ToList();
    }

    public async Task<bool> RemoveClaim(string type, string? value = null)
    {
        ThrowIfDisposed();

        var claims = await UserManager.GetClaimsAsync(_user);
        Claim? claim;
        if (value != null)
            claim = claims.FirstOrDefault(x => x.Type == type && x.Value == value);
        else
            claim = claims.FirstOrDefault(x => x.Type == type);

        if (claim != null)
        {
            var result = await UserManager.RemoveClaimAsync(_user, claim);
            if (result.Succeeded)
                await UpdateClaimsPrincipal();
            return result.Succeeded;
        }
        return false;
    }

    public async Task<bool> RemoveRole(string role)
    {
        ThrowIfDisposed();

        var result = await UserManager.RemoveFromRoleAsync(_user, role);
        if (result.Succeeded)
            await UpdateClaimsPrincipal();
        return result.Succeeded;
    }

    public async Task<bool> RemoveAllClaims()
    {
        ThrowIfDisposed();

        var claims = await UserManager.GetClaimsAsync(_user);
        var result = await UserManager.RemoveClaimsAsync(_user, claims);
        if (result.Succeeded)
            await UpdateClaimsPrincipal();
        return result.Succeeded;
    }

    internal bool InternalHasUpgrade(int upgradeId) => _upgrades.Contains(upgradeId);

    public bool HasUpgrade(int upgradeId)
    {
        ThrowIfDisposed();

        lock (_upgradesLock)
            return InternalHasUpgrade(upgradeId);
    }

    public bool TryAddUpgrade(int upgradeId)
    {
        ThrowIfDisposed();

        lock (_upgradesLock)
        {
            if (InternalHasUpgrade(upgradeId))
                return false;
            _upgrades.Add(upgradeId);
            UpgradeAdded?.Invoke(this, upgradeId);
            return true;
        }
    }

    public bool TryRemoveUpgrade(int upgradeId)
    {
        ThrowIfDisposed();

        lock (_upgradesLock)
        {
            if (!InternalHasUpgrade(upgradeId))
                return false;
            _upgrades.Remove(upgradeId);
            UpgradeRemoved?.Invoke(this, upgradeId);
            return true;
        }
    }

    public void SetSetting(int settingId, string value)
    {
        if (value.Length > 255)
            throw new ArgumentException("Value is too long", nameof(value));
        _settings[settingId] = value;
    }
    
    public string? GetSetting(int settingId)
    {
        if (_settings.TryGetValue(settingId, out var value))
            return value;
        return null;
    }

    public void RemoveSetting(int settingId)
    {
        _settings.TryRemove(settingId, out var _);
    }

}
