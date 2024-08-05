namespace RealmCore.Server.Modules.Players.Persistence;

public interface IUsesUserPersistentData
{
    void LogIn(UserData userData);
    event Action? VersionIncreased;
}

public class PlayerLoggedInEventArgs : EventArgs
{
    public PlayerUserFeature PlayerUserFeature { get; }
    public RealmPlayer Player { get; }

    public PlayerLoggedInEventArgs(PlayerUserFeature playerUserFeature, RealmPlayer player)
    {
        PlayerUserFeature = playerUserFeature;
        Player = player;
    }
}

public class PlayerLoggedOutEventArgs : EventArgs
{
    public PlayerUserFeature PlayerUserFeature { get; }
    public RealmPlayer Player { get; }

    public PlayerLoggedOutEventArgs(PlayerUserFeature playerUserFeature, RealmPlayer player)
    {
        PlayerUserFeature = playerUserFeature;
        Player = player;
    }
}

public sealed class PlayerUserFeature : IPlayerFeature, IDisposable
{
    private readonly object _lock = new();
    private UserData? _user;
    private readonly Dictionary<string, bool> _authorizedPoliciesCache = [];
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly DataEventRepository _userEventRepository;
    private ClaimsPrincipal? _claimsPrincipal;
    private int _version = 0;

    public UserData UserData => _user ?? throw new UserNotSignedInException();
    public ClaimsPrincipal ClaimsPrincipal => _claimsPrincipal ?? throw new UserNotSignedInException();
    public bool IsLoggedIn => _user != null;
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

    public AsyncEvent<PlayerLoggedInEventArgs> LoggedIn { get; set; } = new();
    public AsyncEvent<PlayerLoggedOutEventArgs> LoggedOut { get; set; } = new();

    public RealmPlayer Player { get; init; }

    public PlayerUserFeature(PlayerContext playerContext, IDateTimeProvider dateTimeProvider, DataEventRepository userEventRepository)
    {
        Player = playerContext.Player;
        _dateTimeProvider = dateTimeProvider;
        _userEventRepository = userEventRepository;

        Player.ModelChanged += HandleModelChanged;
    }

    public async Task Login(UserData userData, ClaimsPrincipal claimsPrincipal, bool dontLoadData = false)
    {
        if (userData == null)
            throw new InvalidOperationException();

        lock (_lock)
        {
            if (_user != null)
                throw new InvalidOperationException();

            _user = userData;
            _claimsPrincipal = claimsPrincipal;

            try
            {
                foreach (var playerFeature in Player.GetRequiredService<IEnumerable<IPlayerFeature>>())
                {
                    if(playerFeature is IUsesUserPersistentData usesPlayerPersistentData)
                    {
                        usesPlayerPersistentData.VersionIncreased += IncreaseVersion;
                        if(!dontLoadData)
                            usesPlayerPersistentData.LogIn(userData);
                    }
                }

                var db = Player.GetRequiredService<IDb>();
                db.ChangeTracker.Clear();
                db.Attach(userData);
            }
            catch(Exception)
            {
                _user = null;
                _claimsPrincipal = null;
                throw;
            }
        }

        await LoggedIn.InvokeAsync(this, new PlayerLoggedInEventArgs(this, Player));
    }

    public async Task LogOut()
    {
        lock (_lock)
        {
            if (!IsLoggedIn)
                return;

            if (_user == null)
                throw new InvalidOperationException();

            _user = null;
            _claimsPrincipal = null;
            ClearAuthorizedPoliciesCache();

            foreach (var playerFeature in Player.GetRequiredService<IEnumerable<IPlayerFeature>>())
            {
                if (playerFeature is IUsesUserPersistentData usesPlayerPersistentData)
                {
                    usesPlayerPersistentData.VersionIncreased -= IncreaseVersion;
                }
            }
        }

        await LoggedOut.InvokeAsync(this, new PlayerLoggedOutEventArgs(this, Player));

    }

    public void IncreaseVersion()
    {
        Interlocked.Increment(ref _version);
    }

    public int GetVersion() => _version;

    public bool TryFlushVersion(int minimalVersion)
    {
        if (minimalVersion < 0)
            throw new ArgumentOutOfRangeException();

        if (_version == 0)
            return false;

        if (minimalVersion <= _version)
        {
            Interlocked.Exchange(ref _version, 0);
            return true;
        }
        return false;
    }

    public int[] BlockedUsers()
    {
        lock (_lock)
        {
            if (_user == null)
                throw new InvalidOperationException();

            return _user.BlockedUsers.Select(x => x.UserId2).ToArray();
        }
    }

    private bool IsBlockedCore(int userId)
    {
        if (_user == null)
            throw new InvalidOperationException();

        return _user.BlockedUsers.Where(x => x.UserId2 == userId).Any();
    }
    
    public bool IsBlocked(int userId)
    {
        lock (_lock)
        {
            return IsBlockedCore(userId);
        }
    }

    public bool BlockUser(int userId)
    {
        lock (_lock)
        {
            if (IsBlockedCore(userId))
                return false;

            if (_user == null)
                throw new InvalidOperationException();

            _user.BlockedUsers.Add(new BlockedUserData
            {
                UserId1 = _user.Id,
                UserId2 = userId,
                CreatedAt = _dateTimeProvider.Now
            });
        }

        IncreaseVersion();
        return true;
    }

    public bool UnblockUser(int userId)
    {
        lock (_lock)
        {
            if (_user == null)
                throw new InvalidOperationException();

            var blockedUser = _user.BlockedUsers.FirstOrDefault(x => x.UserId2 == userId);
            if (blockedUser == null)
                return false;

            _user.BlockedUsers.Remove(blockedUser);
        }
        IncreaseVersion();
        return true;
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

    public bool HasAuthorizedPolicies(string[] policies) => policies.All(HasAuthorizedPolicy);

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

    public string[] GetClaims()
    {
        if (_claimsPrincipal != null && _claimsPrincipal.Identity is ClaimsIdentity claimsIdentity)
        {
            return [.. claimsIdentity.Claims.Select(x => x.Type)];
        }

        return [];
    }

    public string[] GetRoles()
    {
        if (_claimsPrincipal != null && _claimsPrincipal.Identity is ClaimsIdentity claimsIdentity)
        {
            return [.. claimsIdentity.Claims.Where(x => x.Type == ClaimTypes.Role).Select(x => x.Value)];
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

    private void HandleModelChanged(Ped sender, ElementChangedEventArgs<Ped, ushort> args)
    {
        if (_user != null)
        {
            _user.Skin = (short)args.NewValue;
        }
    }

    public void Dispose()
    {
        Player.ModelChanged -= HandleModelChanged;
    }
}
