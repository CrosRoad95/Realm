namespace Realm.Server.Elements;

public class RPGPlayer : Player
{
    private readonly CancellationTokenSource _cancellationTokenSource;
    private readonly LuaValueMapper _luaValueMapper;
    private readonly SignInManager<User> _signInManager;
    private readonly UserManager<User> _userManager;
    private readonly EventFunctions _eventFunctions;
    private readonly IdentityFunctions _identityFunctions;

    [NoScriptAccess]
    public CancellationToken CancellationToken { get; private set; }
    [NoScriptAccess]
    public event Action<RPGPlayer, int>? ResourceReady;

    [NoScriptAccess]
    public ClaimsPrincipal? ClaimsPrincipal { get; private set; }

    public bool IsLoggedIn => ClaimsPrincipal != null && ClaimsPrincipal.Identity != null && ClaimsPrincipal.Identity.IsAuthenticated;

    public object? Claims
    {
        get
        {
            if (!IsLoggedIn)
                return null;

            return ClaimsPrincipal!.Claims.Select(x => x.Type).ToArray().ToScriptArray();
        }
    }
    
    public object? Roles
    {
        get
        {
            if (!IsLoggedIn)
                return null;

            return ClaimsPrincipal!.Claims
                .Where(c => c.Type == ClaimTypes.Role)
                .Select(c => c.Value).ToArray().ToScriptArray();
        }
    }

    public RPGPlayer(LuaValueMapper luaValueMapper, SignInManager<User> signInManager, UserManager<User> userManager, EventFunctions eventFunctions, IdentityFunctions identityFunctions)
    {
        _luaValueMapper = luaValueMapper;
        _signInManager = signInManager;
        _userManager = userManager;
        _eventFunctions = eventFunctions;
        _identityFunctions = identityFunctions;
        _cancellationTokenSource = new CancellationTokenSource();
        CancellationToken = _cancellationTokenSource.Token;
        ResourceStarted += RPGPlayer_ResourceStarted;
        Disconnected += RPGPlayer_Disconnected;
    }

    private void RPGPlayer_Disconnected(Player sender, PlayerQuitEventArgs e)
    {
        _cancellationTokenSource.Cancel();
    }

    private void RPGPlayer_ResourceStarted(Player sender, PlayerResourceStartedEventArgs e)
    {
        ResourceReady?.Invoke(sender as RPGPlayer, e.NetId);
    }

    public void Spawn(Spawn spawn)
    {
        Camera.Target = this;
        Camera.Fade(CameraFade.In);
        Spawn(spawn.Position, spawn.Rotation.Z, 0, 0, 0);
    }

    public bool IsPersistant() => true;

    // TODO: improve
    public void TriggerClientEvent(string name, params object[] values)
    {
        LuaValue[] luaValue;
        if (values.Length == 1 && values[0].GetType() == typeof(object[]))
        {
            luaValue = (values[0] as object[]).Select(_luaValueMapper.Map).ToArray();
        }
        else
        {
            luaValue = values.Select(_luaValueMapper.Map).ToArray();
        }
        TriggerLuaEvent(name, this, luaValue);
    }

    public async Task<bool> LogIn(PlayerAccount account, string password)
    {
        if (ClaimsPrincipal != null)
            return false;

        if (await _userManager.CheckPasswordAsync(account.User, password))
        {
            var claimsPrincipal = await _signInManager.CreateUserPrincipalAsync(account.User);
            ClaimsPrincipal = claimsPrincipal;
            await _eventFunctions.InvokeEvent("onPlayerLogin", new PlayerLoggedInEvent
            {
                Player = this,
                Account = account,
            });
            return true;
        }
        return false;
    }

    public async Task<bool> LogOut()
    {
        if (ClaimsPrincipal == null)
            return false;

        ClaimsPrincipal = null;
        await _eventFunctions.InvokeEvent("onPlayerLogout", new PlayerLoggedOutEvent
        {
            Player = this
        });
        return true;
    }

    public async Task<PlayerAccount?> GetAccount()
    {
        if (!IsLoggedIn)
            return null;

        var nameIdentifier = ClaimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);
        return await _identityFunctions.FindAccountById(nameIdentifier);
    }

    public bool IsInRole(string role)
    {
        if (!IsLoggedIn)
            return false;
        return ClaimsPrincipal!.IsInRole(role);
    }

    public bool HasClaim(string type)
    {
        if (!IsLoggedIn)
            return false;
        return ClaimsPrincipal!.HasClaim(x => x.Type == type);
    }

    public string? GetClaimValue(string type)
    {
        if (!IsLoggedIn || !HasClaim(type))
            return null;

        return ClaimsPrincipal!.Claims.First(x => x.Type == type).Value;
    }

    public override string ToString() => "Player";
}
