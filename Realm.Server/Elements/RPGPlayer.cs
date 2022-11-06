using Realm.Common.Utilities;
using SlipeServer.Server.Services;

namespace Realm.Server.Elements;

public class RPGPlayer : Player
{
    private readonly CancellationTokenSource _cancellationTokenSource;
    private readonly MtaServer _mtaServer;
    private readonly LuaValueMapper _luaValueMapper;
    private readonly SignInManager<User> _signInManager;
    private readonly UserManager<User> _userManager;
    private readonly IAuthorizationService _authorizationService;
    private readonly AuthorizationPoliciesProvider _authorizationPoliciesProvider;
    private readonly EventFunctions _eventFunctions;
    private readonly IdentityFunctions _identityFunctions;
    private readonly DebugLog _debugLog;
    private readonly AgnosticGuiSystemService _agnosticGuiSystemService;
    [NoScriptAccess]
    public Latch ResourceStartingLatch = new(3); // TODO: remove hardcoded resources counter
    [NoScriptAccess]
    public CancellationToken CancellationToken { get; private set; }

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


    [NoScriptAccess]
    public event Action<RPGPlayer, bool>? DebugWorldChanged;
    [NoScriptAccess]
    private bool _debugWorld = false;
    public bool DebugWorld
    {
        get => _debugWorld; set
        {
            _debugWorld = value;
            DebugWorldChanged?.Invoke(this, value);
        }
    }

    [NoScriptAccess]
    private bool _debugView = false;
    public bool DebugView { get => _debugView; set
        {
            if(_debugView != value)
            {
                _debugView = value;
                _debugLog.SetVisibleTo(this, value);
            }
        }
    }

    public RPGPlayer(MtaServer mtaServer, LuaValueMapper luaValueMapper, SignInManager<User> signInManager,
        UserManager<User> userManager, IAuthorizationService authorizationService,
        AuthorizationPoliciesProvider authorizationPoliciesProvider, EventFunctions eventFunctions,
        IdentityFunctions identityFunctions, DebugLog debugLog,
        AgnosticGuiSystemService agnosticGuiSystemService)
    {
        _mtaServer = mtaServer;
        _luaValueMapper = luaValueMapper;
        _signInManager = signInManager;
        _userManager = userManager;
        _authorizationService = authorizationService;
        _authorizationPoliciesProvider = authorizationPoliciesProvider;
        _eventFunctions = eventFunctions;
        _identityFunctions = identityFunctions;
        _debugLog = debugLog;
        _agnosticGuiSystemService = agnosticGuiSystemService;
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
        ResourceStartingLatch.Decrement();
    }

    public virtual async Task<bool> Spawn(Spawn spawn)
    {
        if(await spawn.Authorize(this))
        {
            Camera.Target = this;
            Camera.Fade(CameraFade.In);
            Spawn(spawn.Position, spawn.Rotation.Z, 0, 0, 0);
            await _eventFunctions.InvokeEvent(new PlayerSpawned
            {
                Player = this,
                Spawn = spawn,
            });
            return true;
        }
        return false;
    }

    public bool IsPersistant() => true;

    // TODO: improve
    public void TriggerClientEvent(string name, params object[] values)
    {
        LuaValue[] luaValue;
        if (values.Length == 1 && values[0].GetType() == typeof(object[]))
        {
            luaValue = ((object[])values[0]).Select(_luaValueMapper.Map).ToArray();
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
            await _eventFunctions.InvokeEvent(new PlayerLoggedInEvent
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
        await _eventFunctions.InvokeEvent(new PlayerLoggedOutEvent
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

    [NoScriptAccess]
    public async Task<AuthorizationResult?> AuthorizeInternal(string policy)
    {
        _authorizationPoliciesProvider.ValidatePolicy(policy);
        if (!IsLoggedIn)
            return null;

        var result = await _authorizationService.AuthorizeAsync(ClaimsPrincipal!, policy);

        return result;
    }

    public async Task<bool> Authorize(string policy)
    {
        var result = await AuthorizeInternal(policy);

        return result?.Succeeded ?? false;
    }

    public bool OpenGui(string gui) => _agnosticGuiSystemService.OpenGui(this, gui);
    public bool CloseGui(string gui) => _agnosticGuiSystemService.CloseGui(this, gui);
    public void CloseAllGuis() => _agnosticGuiSystemService.CloseAllGuis(this);

    [NoScriptAccess]
    public void Reset()
    {
        Camera.Fade(CameraFade.Out, 0, System.Drawing.Color.Black);
        Camera.Target = null;
        ClaimsPrincipal = null;
        ResourceStartingLatch = new(3); // TODO: remove hardcoded resources counter
        DebugView = false;
    }

    public override string ToString() => Name;
}
