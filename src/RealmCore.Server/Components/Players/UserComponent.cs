namespace RealmCore.Server.Components.Players;

[ComponentUsage(false)]
public class UserComponent : AsyncComponent
{
    private readonly UserData? _user;
    private readonly SignInManager<UserData> _signInManager;
    private readonly UserManager<UserData> _userManager;
    private readonly List<int> _upgrades = new();
    private readonly object _upgradesLock = new();
    private readonly ConcurrentDictionary<int, string> _settings = new();
    private ClaimsPrincipal _claimsPrincipal = default!;

    public UserData User => _user ?? throw new InvalidOperationException();
    public ClaimsPrincipal ClaimsPrincipal => _claimsPrincipal ?? throw new ArgumentNullException(nameof(_claimsPrincipal));
    public int Id => _user?.Id ?? -1;
    public string? Nick => _user?.Nick;
    public string? UserName => _user?.UserName;
    public IReadOnlyList<int> Upgrades => _upgrades;
    public ICollection<int> Settings => _settings.Keys;

    public event Action<UserComponent, int>? UpgradeAdded;
    public event Action<UserComponent, int>? UpgradeRemoved;
    public event Action<UserComponent, ClaimsPrincipal>? ClaimsPrincipalUpdated;
    public event Action<UserComponent, int, string>? SettingChanged;
    public event Action<UserComponent, int, string>? SettingRemoved;
    private readonly List<string> _roles = new();

    internal UserComponent(UserData user, SignInManager<UserData> signInManager, UserManager<UserData> userManager)
    {
        _user = user;
        _signInManager = signInManager;
        _userManager = userManager;
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
        if (_user == null || _signInManager == null)
            return;

        _roles.Clear();
        _roles.AddRange(await GetRolesAsync());
        _claimsPrincipal = await _signInManager.CreateUserPrincipalAsync(_user);
        foreach (var role in _roles)
        {
            if(_claimsPrincipal.Identity is ClaimsIdentity claimsIdentity)
                claimsIdentity.AddClaim(new Claim(ClaimTypes.Role, role));
        }
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

        if (_claimsPrincipal == null)
            return false;
        return _claimsPrincipal.HasClaim(x => x.Type == type);
    }

    public string? GetClaimValue(string type)
    {
        ThrowIfDisposed();

        if (_claimsPrincipal == null)
            return null;
        return _claimsPrincipal.Claims.First(x => x.Type == type).Value;
    }

    public async Task<bool> AddClaim(string type, string value)
    {
        ThrowIfDisposed();

        if (_user == null)
            return false;

        var result = await _userManager.AddClaimAsync(_user, new Claim(type, value));
        if (result.Succeeded)
            await UpdateClaimsPrincipal();
        return result.Succeeded;
    }

    public async Task<bool> AddClaims(Dictionary<string, string> claims)
    {
        ThrowIfDisposed();

        if (_user == null)
            return false;

        var result = await _userManager.AddClaimsAsync(_user, claims.Select(x => new Claim(x.Key, x.Value)));
        if (result.Succeeded)
            await UpdateClaimsPrincipal();
        return result.Succeeded;
    }

    public async Task<bool> AddRole(string role)
    {
        ThrowIfDisposed();

        if (_roles.Contains(role))
            return false;

        if (_user == null)
            return false;

        var result = await _userManager.AddToRoleAsync(_user, role);
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

        if (_user == null)
            return false;

        var result = await _userManager.AddToRolesAsync(_user, rolesToAdd);
        if (result.Succeeded)
            await UpdateClaimsPrincipal();
        return result.Succeeded;
    }

    public async Task<IEnumerable<string>> GetClaims()
    {
        ThrowIfDisposed();

        if (_user == null || _userManager == null)
            return new List<string>().AsReadOnly();

        return (await _userManager.GetClaimsAsync(_user)).Select(x => x.Type).ToList();
    }

    public async Task<IReadOnlyList<string>> GetRolesAsync()
    {
        ThrowIfDisposed();

        if (_user == null || _userManager == null)
            return new List<string>().AsReadOnly();

        return (await _userManager.GetRolesAsync(_user)).ToList().AsReadOnly();
    }

    public IReadOnlyList<string> GetRoles()
    {
        ThrowIfDisposed();

        return new List<string>(_roles).AsReadOnly();
    }

    public async Task<bool> RemoveClaim(string type, string? value = null)
    {
        ThrowIfDisposed();

        if (_user == null)
            return false;

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

    public async Task<bool> RemoveRole(string role)
    {
        ThrowIfDisposed();

        if (!_roles.Contains(role))
            return false;

        if (_user == null)
            return false;

        var result = await _userManager.RemoveFromRoleAsync(_user, role);
        if (result.Succeeded)
            await UpdateClaimsPrincipal();
        return result.Succeeded;
    }

    public async Task<bool> RemoveAllClaims()
    {
        ThrowIfDisposed();

        if (_user == null)
            return false;

        var claims = await _userManager.GetClaimsAsync(_user);
        var result = await _userManager.RemoveClaimsAsync(_user, claims);
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

        _settings[settingId] = value;
        SettingChanged?.Invoke(this, settingId, value);
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

        if(_settings.TryRemove(settingId, out var value))
            SettingRemoved?.Invoke(this, settingId, value);
    }
}
