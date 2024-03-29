﻿namespace RealmCore.Server.Modules.Players.Persistence;

public interface IUsesUserPersistentData
{
    void SignIn(UserData userData);
    void SignOut();
    event Action? VersionIncreased;
}

public interface IPlayerUserFeature : IPlayerFeature
{
    internal UserData UserData { get; }
    ClaimsPrincipal ClaimsPrincipal { get; }
    int Id { get; }
    string Nick { get; }
    string UserName { get; }
    DateTime? LastNewsReadDateTime { get; }
    bool IsSignedIn { get; }
    DateTime? RegisteredDateTime { get; }
    string[] AuthorizedPolicies { get; }

    event Action<IPlayerUserFeature, RealmPlayer>? SignedIn;
    event Action<IPlayerUserFeature, RealmPlayer>? SignedOut;

    IReadOnlyList<string> GetClaims();
    IReadOnlyList<string> GetRoles();
    string? GetClaimValue(string type);
    bool HasAuthorizedPolicies(string[] policies);
    bool HasAuthorizedPolicy(string policy);
    bool HasClaim(string type, string? value = null);
    bool IsInRole(string role);
    void SignIn(UserData user, ClaimsPrincipal claimsPrincipal);
    void SignOut();
    bool TryRemoveClaim(string type, string? value = null);
    bool TryRemoveRole(string role);
    void UpdateLastNewsRead();
    void IncreaseVersion();
    int GetVersion();
    bool TryFlushVersion(int minimalVersion);
    internal bool AddClaim(string type, string value);
    internal bool AddClaims(Dictionary<string, string> claims);
    internal bool AddRole(string role);
    internal bool AddRoles(IEnumerable<string> roles);
    internal void SetAuthorizedPolicyState(string policy, bool authorized);
}

internal sealed class PlayerUserFeature : IPlayerUserFeature
{
    private readonly object _lock = new();
    private UserData? _user;
    private readonly Dictionary<string, bool> _authorizedPoliciesCache = [];
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IUserEventRepository _userEventRepository;
    private ClaimsPrincipal? _claimsPrincipal;
    private int _version = 0;

    public UserData UserData => _user ?? throw new UserNotSignedInException();
    public ClaimsPrincipal ClaimsPrincipal => _claimsPrincipal ?? throw new UserNotSignedInException();
    public bool IsSignedIn => _user != null;
    public int Id => _user?.Id ?? -1;
    public string Nick => _user?.Nick ?? throw new UserNotSignedInException();
    public string UserName => _user?.UserName ?? throw new UserNotSignedInException();
    public DateTime? LastNewsReadDateTime => UserData.LastNewsReadDateTime;
    public DateTime? RegisteredDateTime => UserData.RegisteredDateTime;

    public string[] AuthorizedPolicies
    {
        get
        {
            lock (_lock)
                return _authorizedPoliciesCache.Where(x => x.Value).Select(x => x.Key).ToArray();
        }
    }

    public event Action<IPlayerUserFeature, RealmPlayer>? SignedIn;
    public event Action<IPlayerUserFeature, RealmPlayer>? SignedOut;

    public RealmPlayer Player { get; init; }

    public PlayerUserFeature(PlayerContext playerContext, IDateTimeProvider dateTimeProvider, IUserEventRepository userEventRepository)
    {
        Player = playerContext.Player;
        _dateTimeProvider = dateTimeProvider;
        _userEventRepository = userEventRepository;
    }

    public void SignIn(UserData user, ClaimsPrincipal claimsPrincipal)
    {
        if (user == null)
            throw new InvalidOperationException();

        lock (_lock)
        {
            _user = user;
            _claimsPrincipal = claimsPrincipal;
            foreach (var playerFeature in Player.GetRequiredService<IEnumerable<IPlayerFeature>>())
            {
                if(playerFeature is IUsesUserPersistentData usesPlayerPersistentData)
                    usesPlayerPersistentData.SignIn(user);
            }
            SignedIn?.Invoke(this, Player);
        }
    }

    public void SignOut()
    {
        lock (_lock)
        {
            if (IsSignedIn)
            {
                if (_user == null)
                    throw new InvalidOperationException();
                _user = null;
                _claimsPrincipal = null;
                SignedOut?.Invoke(this, Player);
                ClearAuthorizedPoliciesCache();

                foreach (var playerFeature in Player.GetRequiredService<IEnumerable<IPlayerFeature>>())
                {
                    if (playerFeature is IUsesUserPersistentData usesPlayerPersistentData)
                        usesPlayerPersistentData.SignOut();
                }
            }
        }
    }

    public void IncreaseVersion()
    {
        Interlocked.Increment(ref _version);
    }

    public int GetVersion() => _version;

    public bool TryFlushVersion(int minimalVersion)
    {
        if (minimalVersion <= _version)
        {
            Interlocked.Exchange(ref _version, 0);
            return true;
        }
        return false;
    }

    public void SetAuthorizedPolicyState(string policy, bool authorized)
    {
        lock (_lock)
        {
            _authorizedPoliciesCache[policy] = authorized;
        }
    }

    public bool HasAuthorizedPolicy(string policy)
    {
        lock (_lock)
        {
            if (_authorizedPoliciesCache.TryGetValue(policy, out var authorized))
                return authorized;
            return false;
        }
    }

    public bool HasAuthorizedPolicies(string[] policies)
    {
        lock (_lock)
        {
            foreach (var policy in policies)
            {
                if (_authorizedPoliciesCache.TryGetValue(policy, out var authorized) && !authorized)
                    return false;
            }
            return true;
        }
    }

    private void ClearAuthorizedPoliciesCache()
    {
        lock (_lock)
            _authorizedPoliciesCache.Clear();
    }

    public bool IsInRole(string role)
    {
        return HasClaim(ClaimTypes.Role, role);
    }

    public bool HasClaim(string type, string? value = null)
    {
        if (_claimsPrincipal == null)
            return false;
        if (value != null)
            return _claimsPrincipal.HasClaim(x => x.Type == type && x.Value == type);
        return _claimsPrincipal.HasClaim(x => x.Type == type);
    }

    public string? GetClaimValue(string type)
    {
        if (_claimsPrincipal == null)
            return null;
        return _claimsPrincipal.Claims.First(x => x.Type == type).Value;
    }

    public bool AddClaim(string type, string value)
    {
        if (_claimsPrincipal == null)
            return false;

        if (_claimsPrincipal.Identity is ClaimsIdentity claimsIdentity)
        {
            claimsIdentity.AddClaim(new Claim(type, value));
            ClearAuthorizedPoliciesCache();
            return true;
        }
        return false;
    }

    public bool AddClaims(Dictionary<string, string> claims)
    {
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
        if (_claimsPrincipal != null && _claimsPrincipal.Identity is ClaimsIdentity claimsIdentity)
        {
            return claimsIdentity.Claims.Select(x => x.Type).ToList();
        }

        return [];
    }

    public IReadOnlyList<string> GetRoles()
    {
        if (_claimsPrincipal != null && _claimsPrincipal.Identity is ClaimsIdentity claimsIdentity)
        {
            return claimsIdentity.Claims.Where(x => x.Type == ClaimTypes.Role).Select(x => x.Value).ToList();
        }

        return [];
    }

    public bool TryRemoveClaim(string type, string? value = null)
    {
        if (_user == null)
            return false;

        if (_claimsPrincipal != null && _claimsPrincipal.Identity is ClaimsIdentity claimsIdentity)
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
        return TryRemoveClaim(ClaimTypes.Role, role);
    }

    public void UpdateLastNewsRead()
    {
        UserData.LastNewsReadDateTime = _dateTimeProvider.Now;
    }
}
