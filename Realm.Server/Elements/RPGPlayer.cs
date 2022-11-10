using Realm.Common.Utilities;
using Realm.Server.Extensions;
using Realm.Server.Services;
using SlipeServer.Server.Services;

namespace Realm.Server.Elements;

public class RPGPlayer : Player
{
    private readonly CancellationTokenSource _cancellationTokenSource;
    private readonly MtaServer _mtaServer;
    private readonly LuaValueMapper _luaValueMapper;
    private readonly EventFunctions _eventFunctions;
    private readonly DebugLog _debugLog;
    private readonly AgnosticGuiSystemService _agnosticGuiSystemService;
    private readonly AccountsInUseService _accountsInUseService;
    [NoScriptAccess]
    public Latch ResourceStartingLatch = new(3); // TODO: remove hardcoded resources counter
    [NoScriptAccess]
    public CancellationToken CancellationToken { get; private set; }

    public PlayerAccount? Account { get; private set; }

    public bool IsLoggedIn => Account != null && Account.IsAuthenticated;

    [NoScriptAccess]
    public event Action<RPGPlayer, bool>? DebugWorldChanged;
    [NoScriptAccess]
    public event Action<RPGPlayer, string>? LoggedIn;
    public event Action<RPGPlayer, string>? LoggedOut;
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

    public RPGPlayer(MtaServer mtaServer, LuaValueMapper luaValueMapper, EventFunctions eventFunctions,
        DebugLog debugLog, AgnosticGuiSystemService agnosticGuiSystemService, AccountsInUseService accountsInUseService)
    {
        _mtaServer = mtaServer;
        _luaValueMapper = luaValueMapper;
        _eventFunctions = eventFunctions;
        _debugLog = debugLog;
        _agnosticGuiSystemService = agnosticGuiSystemService;
        _accountsInUseService = accountsInUseService;
        _cancellationTokenSource = new CancellationTokenSource();
        CancellationToken = _cancellationTokenSource.Token;
        ResourceStarted += RPGPlayer_ResourceStarted;
        Disconnected += RPGPlayer_Disconnected;
    }

    private async void RPGPlayer_Disconnected(Player sender, PlayerQuitEventArgs e)
    {
        await LogOut();
        _cancellationTokenSource.Cancel();
    }

    private void RPGPlayer_ResourceStarted(Player sender, PlayerResourceStartedEventArgs e)
    {
        ResourceStartingLatch.Decrement();
    }

    public virtual async Task<bool> Spawn(Spawn spawn)
    {
        if(await spawn.IsAuthorized(this))
        {
            Camera.Target = this;
            Camera.Fade(CameraFade.In);
            Spawn(spawn.Position, spawn.Rotation.Z, 0, 0, 0);
            using var playerSpawnedEvent = new PlayerSpawnedEvent(this, spawn);
            await _eventFunctions.InvokeEvent(playerSpawnedEvent);
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
        if (IsLoggedIn)
            return false;

        if (!await account.CheckPasswordAsync(password))
            return false;

        if (!_accountsInUseService.AssignPlayerToAccountId(this, account.Id))
            return false;

        await account.SignIn();

        Account = account;
        using var playerLoggedInEvent = new PlayerLoggedInEvent(this, account);
        await _eventFunctions.InvokeEvent(playerLoggedInEvent);
        LoggedIn?.Invoke(this, Account.Id);
        return true;
    }

    public async Task<bool> LogOut()
    {
        if (!IsLoggedIn)
            return false;

        using var playerLoggedOutEvent = new PlayerLoggedOutEvent(this);
        await _eventFunctions.InvokeEvent(playerLoggedOutEvent);
        var id = Account!.Id;
        Account = null;
        LoggedOut?.Invoke(this, id);
        return true;
    }

    public bool OpenGui(string gui) => _agnosticGuiSystemService.OpenGui(this, gui);
    public bool CloseGui(string gui) => _agnosticGuiSystemService.CloseGui(this, gui);
    public void CloseAllGuis() => _agnosticGuiSystemService.CloseAllGuis(this);

    [NoScriptAccess]
    public void Reset()
    {
        Camera.Fade(CameraFade.Out, 0, System.Drawing.Color.Black);
        Camera.Target = null;
        Account = null;
        ResourceStartingLatch = new(3); // TODO: remove hardcoded resources counter
        DebugView = false;
    }

    public override string ToString() => Name;
}
