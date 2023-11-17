using RealmCore.Persistence.Data.Helpers;

namespace RealmCore.Server.Services.Players;

internal sealed class PlayerUserService : IPlayerUserService, IDisposable
{
    private struct PolicyCache
    {
        public string policy;
        public bool authorized;
    }

    private UserData? _user;
    private readonly object _lock = new();
    private readonly List<PolicyCache> _authorizedPolicies = new();
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IUserEventRepository _userEventRepository;
    private ClaimsPrincipal? _claimsPrincipal;
    private int _version = 0;

    public UserData User => _user ?? throw new UserNotSignedInException();
    public ClaimsPrincipal ClaimsPrincipal => _claimsPrincipal ?? throw new UserNotSignedInException();
    public bool IsSignedIn => _user != null;
    public int Id => _user?.Id ?? -1;
    public string Nick => _user?.Nick ?? throw new UserNotSignedInException();
    public string UserName => _user?.UserName ?? throw new UserNotSignedInException();
    public DateTime? LastNewsReadDateTime => User.LastNewsReadDateTime;
    public DateTime? RegisteredDateTime => User.RegisteredDateTime;
    public TransformAndMotion? LastTransformAndMotion { get => User.LastTransformAndMotion; set => User.LastTransformAndMotion = value; }

    public event Action<IPlayerUserService, RealmPlayer>? SignedIn;
    public event Action<IPlayerUserService, RealmPlayer>? SignedOut;

    public RealmPlayer Player { get; }
    public PlayerUserService(PlayerContext playerContext, IDateTimeProvider dateTimeProvider, IUserEventRepository userEventRepository)
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
            SignedIn?.Invoke(this, Player);
        }
    }

    public void SignOut()
    {
        lock (_lock)
        {
            if (_user == null)
                throw new InvalidOperationException();
            _user = null;
            _claimsPrincipal = null;
            SignedOut?.Invoke(this, Player);
        }
        ClearAuthorizedPoliciesCache();
    }

    public void IncreaseVersion()
    {
        Interlocked.Increment(ref _version);
    }

    public int GetVersion() => _version;
    public bool TryFlushVersion(int minimalVersion)
    {
        if(minimalVersion >= _version)
        {
            Interlocked.Exchange(ref _version, 0);
            return true;
        }
        return false;
    }

    public void AddAuthorizedPolicy(string policy, bool authorized)
    {
        lock (_lock)
        {
            _authorizedPolicies.Add(new PolicyCache
            {
                policy = policy,
                authorized = authorized
            });
        }
    }

    public bool HasAuthorizedPolicy(string policy, out bool authorized)
    {
        lock (_lock)
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

    public bool HasAuthorizedPolicies(string[] policies)
    {
        lock (_lock)
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
        _authorizedPolicies.Clear();
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
        if (_claimsPrincipal.Identity is ClaimsIdentity claimsIdentity)
        {
            return claimsIdentity.Claims.Select(x => x.Type).ToList();
        }

        return [];
    }

    public IReadOnlyList<string> GetRoles()
    {
        if (_claimsPrincipal.Identity is ClaimsIdentity claimsIdentity)
        {
            return claimsIdentity.Claims.Where(x => x.Type == ClaimTypes.Role).Select(x => x.Value).ToList();
        }

        return [];
    }

    public bool TryRemoveClaim(string type, string? value = null)
    {
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
        return TryRemoveClaim(ClaimTypes.Role, role);
    }

    public void UpdateLastNewsRead()
    {
        User.LastNewsReadDateTime = _dateTimeProvider.Now;
    }

    public void Dispose()
    {
        if(IsSignedIn)
            SignOut();
    }
}
