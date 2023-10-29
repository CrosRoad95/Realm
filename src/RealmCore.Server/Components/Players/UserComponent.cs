using RealmCore.Server.DomainObjects;

namespace RealmCore.Server.Components.Players;

[ComponentUsage(false)]
public class UserComponent : Component
{
    private struct PolicyCache
    {
        public string policy;
        public bool authorized;
    }

    private readonly UserData? _user;
    private readonly List<int> _upgrades = new();
    private readonly object _upgradesLock = new();
    private readonly ConcurrentDictionary<int, string> _settings = new();
    private readonly ClaimsPrincipal _claimsPrincipal;

    public UserData User => _user ?? throw new InvalidOperationException();
    public ClaimsPrincipal ClaimsPrincipal => _claimsPrincipal;
    public int Id => _user?.Id ?? -1;
    public string? Nick => _user?.Nick;
    public string? UserName => _user?.UserName;
    public IReadOnlyList<int> Upgrades => _upgrades;
    public ICollection<int> Settings => _settings.Keys;
    public DateTime? LastNewsReadDateTime
    {
        get
        {
            return User.LastNewsReadDateTime;
        }
        internal set
        {
            User.LastNewsReadDateTime = value;
        }
    }

    public Bans Bans { get; }

    public event Action<UserComponent, int>? UpgradeAdded;
    public event Action<UserComponent, int>? UpgradeRemoved;
    public event Action<UserComponent, int, string>? SettingChanged;
    public event Action<UserComponent, int, string>? SettingRemoved;
    private readonly object _authorizedPoliciesLock = new();
    private readonly List<PolicyCache> _authorizedPolicies = new();

    internal UserComponent(UserData user, ClaimsPrincipal claimsPrincipal, Bans bans)
    {
        _user = user;
        _claimsPrincipal = claimsPrincipal;
        Bans = bans;
        _upgrades = _user.Upgrades.Select(x => x.UpgradeId).ToList();
        foreach (var item in _user.Settings)
        {
            _settings[item.SettingId] = item.Value;
        }
    }

    internal void AddAuthorizedPolicy(string policy, bool authorized)
    {
        lock (_authorizedPoliciesLock)
        {
            _authorizedPolicies.Add(new PolicyCache
            {
                policy = policy,
                authorized = authorized
            });
        }
    }

    internal bool HasAuthorizedPolicy(string policy, out bool authorized)
    {
        lock (_authorizedPoliciesLock)
        {
            var index = _authorizedPolicies.FindIndex(x => x.policy == policy);
            if (index == -1)
            {
                authorized = false;
                return false;
            }

            authorized = _authorizedPolicies[index].authorized;
            return true;
        }
    }
    
    internal bool HasAuthorizedPolicies(string[] policies)
    {
        lock (_authorizedPoliciesLock)
        {
            foreach (var policy in policies)
            {
                var index = _authorizedPolicies.FindIndex(x => x.policy == policy);
                if (index == -1)
                    return false;

                if (!_authorizedPolicies[index].authorized)
                    return false;
            }
            return true;
        }
    }

    private void ClearAuthorizedPoliciesCache()
    {
        lock (_authorizedPoliciesLock)
            _authorizedPolicies.Clear();
    }

    public bool IsInRole(string role)
    {
        ThrowIfDisposed();

        return HasClaim(ClaimTypes.Role, role);
    }

    public bool HasClaim(string type, string? value = null)
    {
        ThrowIfDisposed();

        if (_claimsPrincipal == null)
            return false;
        if(value != null)
            return _claimsPrincipal.HasClaim(x => x.Type == type && x.Value == type);
        return _claimsPrincipal.HasClaim(x => x.Type == type);
    }

    public string? GetClaimValue(string type)
    {
        ThrowIfDisposed();

        if (_claimsPrincipal == null)
            return null;
        return _claimsPrincipal.Claims.First(x => x.Type == type).Value;
    }

    public bool AddClaim(string type, string value)
    {
        ThrowIfDisposed();

        if (_claimsPrincipal == null)
            return false;

        if(_claimsPrincipal.Identity is ClaimsIdentity claimsIdentity)
        { 
            claimsIdentity.AddClaim(new Claim(type, value));
            ClearAuthorizedPoliciesCache();
            return true;
        }
        return false;
    }

    public bool AddClaims(Dictionary<string, string> claims)
    {
        ThrowIfDisposed();

        if (_claimsPrincipal == null)
            return false;

        if (_claimsPrincipal.Identity is ClaimsIdentity claimsIdentity)
        {
            claimsIdentity.AddClaims(claims.Select(x => new Claim(x.Key, x.Value)));
            ClearAuthorizedPoliciesCache();
            return true;
        }
        return false;
    }

    public bool AddRole(string role)
    {
        ThrowIfDisposed();

        if (_claimsPrincipal == null)
            return false;

        if (_claimsPrincipal.Identity is ClaimsIdentity claimsIdentity)
        {
            claimsIdentity.AddClaim(new Claim(ClaimTypes.Role, role));
            ClearAuthorizedPoliciesCache();
            return true;
        }
        return false;
    }

    public bool AddRoles(IEnumerable<string> roles)
    {
        ThrowIfDisposed();

        if (_claimsPrincipal == null)
            return false;

        if (_claimsPrincipal.Identity is ClaimsIdentity claimsIdentity)
        {
            claimsIdentity.AddClaims(roles.Select(x => new Claim(ClaimTypes.Role, x)));
            ClearAuthorizedPoliciesCache();
            return true;
        }
        return false;
    }

    public IReadOnlyList<string> GetClaims()
    {
        ThrowIfDisposed();

        if (_claimsPrincipal.Identity is ClaimsIdentity claimsIdentity)
        {
            return claimsIdentity.Claims.Select(x => x.Type).ToList();
        }

        return [];
    }

    public IReadOnlyList<string> GetRoles()
    {
        ThrowIfDisposed();

        if (_claimsPrincipal.Identity is ClaimsIdentity claimsIdentity)
        {
            return claimsIdentity.Claims.Where(x => x.Type == ClaimTypes.Role).Select(x => x.Value).ToList();
        }

        return [];
    }

    public bool TryRemoveClaim(string type, string? value = null)
    {
        ThrowIfDisposed();

        if (_user == null)
            return false;

        if (_claimsPrincipal.Identity is ClaimsIdentity claimsIdentity)
        {
            Claim? claim = null;
            if (value != null)
                claim = claimsIdentity.Claims.FirstOrDefault(x => x.Type == type && x.Value == value);
            else
                claim = claimsIdentity.Claims.FirstOrDefault(x => x.Type == type);

            if (claimsIdentity.TryRemoveClaim(claim))
            {
                ClearAuthorizedPoliciesCache();
                return true;
            }
            return false;
        }
        return false;
    }

    public bool TryRemoveRole(string role)
    {
        ThrowIfDisposed();

        return TryRemoveClaim(ClaimTypes.Role, role);
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

    public bool TryGetSetting(int settingId, out string? value)
    {
        ThrowIfDisposed();
        if (_settings.TryGetValue(settingId, out value))
            return true;
        return false;
    }

    public bool RemoveSetting(int settingId)
    {
        ThrowIfDisposed();

        if(_settings.TryRemove(settingId, out var value))
        {
            SettingRemoved?.Invoke(this, settingId, value);
            return true;
        }
        return false;
    }

    public override void Dispose()
    {
        Bans.Dispose();
    }
}
