﻿namespace RealmCore.Server.Components.Players;

[ComponentUsage(false)]
public class UserComponent : AsyncComponent
{
    [Inject]
    private UserManager<UserData> UserManager { get; set; } = default!;
    [Inject]
    private SignInManager<UserData> SignInManager { get; set; } = default!;

    private readonly UserData _user;
    private ClaimsPrincipal _claimsPrincipal = default!;
    private List<int> _upgrades = new();
    private object _upgradesLock = new();
    private readonly ConcurrentDictionary<int, string> _settings = new();

    internal UserData User => _user;
    public ClaimsPrincipal ClaimsPrincipal => _claimsPrincipal ?? throw new ArgumentNullException(nameof(_claimsPrincipal));
    public int Id => _user.Id;
    public string? UserName => _user.UserName;
    public IReadOnlyList<int> Upgrades => _upgrades;
    public ICollection<int> Settings => _settings.Keys;

    public event Action<UserComponent, int>? UpgradeAdded;
    public event Action<UserComponent, int>? UpgradeRemoved;
    public event Action<UserComponent, ClaimsPrincipal>? ClaimsPrincipalUpdated;
    private readonly List<string> _roles = new();

    internal UserComponent(UserData user)
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
        _roles.AddRange(await GetRolesAsync());
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

        return _roles.Contains(role);
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

        if (_roles.Contains(role))
            return false;

        var result = await UserManager.AddToRoleAsync(_user, role);
        if (result.Succeeded)
            await UpdateClaimsPrincipal();
        return result.Succeeded;
    }

    public async Task<bool> AddRoles(IEnumerable<string> roles)
    {
        ThrowIfDisposed();

        var rolesToAdd = roles.Except(_roles);

        if (!rolesToAdd.Any())
            return false;

        var result = await UserManager.AddToRolesAsync(_user, rolesToAdd);
        if (result.Succeeded)
            await UpdateClaimsPrincipal();
        return result.Succeeded;
    }

    public async Task<IEnumerable<string>> GetClaims()
    {
        ThrowIfDisposed();

        return (await UserManager.GetClaimsAsync(_user)).Select(x => x.Type).ToList();
    }

    public async Task<IReadOnlyList<string>> GetRolesAsync()
    {
        ThrowIfDisposed();

        return (await UserManager.GetRolesAsync(_user)).ToList().AsReadOnly();
    }

    public IReadOnlyList<string> GetRoles()
    {
        ThrowIfDisposed();

        return new List<string>(_roles).AsReadOnly();
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

        if (!_roles.Contains(role))
            return false;

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
        ThrowIfDisposed();

        if (value.Length > 255)
            throw new ArgumentException("Value is too long", nameof(value));
        _settings[settingId] = value;
    }

    public string? GetSetting(int settingId)
    {
        ThrowIfDisposed();
        if (_settings.TryGetValue(settingId, out var value))
            return value;
        return null;
    }

    public void RemoveSetting(int settingId)
    {
        ThrowIfDisposed();

        _settings.TryRemove(settingId, out var _);
    }
}