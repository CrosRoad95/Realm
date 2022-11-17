using Newtonsoft.Json.Linq;
using Realm.Server.Scripting.Sessions;

namespace Realm.Server.Elements;

[NoDefaultScriptAccess]
public class RPGPlayer : Player
{
    private readonly CancellationTokenSource _cancellationTokenSource;
    private readonly LuaValueMapper _luaValueMapper;
    private readonly EventScriptingFunctions _eventFunctions;
    private readonly DebugLog _debugLog;
    private readonly AgnosticGuiSystemService _agnosticGuiSystemService;
    private readonly AccountsInUseService _accountsInUseService;
    private readonly LuaInteropService _luaInteropService;
    private readonly ChatBox _chatBox;
    private readonly GameplayScriptingFunctions _gameplayScriptingFunctions;
    private readonly ILogger _logger;
    public Latch ResourceStartingLatch = new(3); // TODO: remove hardcoded resources counter
    public CancellationToken CancellationToken { get; private set; }

    [ScriptMember("account", ScriptAccess.ReadOnly)]
    public PlayerAccount? Account { get; private set; }
    [ScriptMember("language", ScriptAccess.ReadOnly)]
    public string Language { get; private set; } = "pl";
    [ScriptMember("isLoggedIn", ScriptAccess.ReadOnly)]
    public bool IsLoggedIn => Account != null && Account.IsAuthenticated;

    public event Action<RPGPlayer, bool>? DebugWorldChanged;
    public event Action<RPGPlayer, string>? LoggedIn;
    public event Action<RPGPlayer, string>? LoggedOut;
    private bool _debugWorld = false;

    [ScriptMember("debugWorld")]
    public bool DebugWorld
    {
        get => _debugWorld; set
        {
            _debugWorld = value;
            DebugWorldChanged?.Invoke(this, value);
            if (value)
                _logger.Verbose("Enabled world debug");
            else
                _logger.Verbose("Disabled world debug");
        }
    }

    private bool _debugView = false;
    [ScriptMember("debugView")]
    public bool DebugView { get => _debugView; set
        {
            if(_debugView != value)
            {
                _debugView = value;
                _debugLog.SetVisibleTo(this, value);
                if(value)
                    _logger.Verbose("Enabled debug view");
                else
                    _logger.Verbose("Disabled debug view");
            }
        }
    }

    private readonly List<SessionBase> _runningSessions = new();

    public RPGPlayer(LuaValueMapper luaValueMapper, EventScriptingFunctions eventFunctions,
        DebugLog debugLog, AgnosticGuiSystemService agnosticGuiSystemService, AccountsInUseService accountsInUseService,
        ILogger logger, LuaInteropService luaInteropService, ChatBox chatBox, GameplayScriptingFunctions gameplayScriptingFunctions)
    {
        _luaValueMapper = luaValueMapper;
        _eventFunctions = eventFunctions;
        _debugLog = debugLog;
        _agnosticGuiSystemService = agnosticGuiSystemService;
        _accountsInUseService = accountsInUseService;
        _luaInteropService = luaInteropService;
        _chatBox = chatBox;
        _gameplayScriptingFunctions = gameplayScriptingFunctions;
        _logger = logger
            .ForContext<RPGPlayer>()
            .ForContext(new RPGPlayerEnricher(this));
        _cancellationTokenSource = new CancellationTokenSource();
        CancellationToken = _cancellationTokenSource.Token;
        ResourceStarted += RPGPlayer_ResourceStarted;
        Disconnected += RPGPlayer_Disconnected;

        _luaInteropService.ClientCultureInfoUpdate += HandleClientCultureInfoUpdate;
    }

    private void HandleClientCultureInfoUpdate(Player player, CultureInfo culture)
    {
        if (player == this)
        {
            _luaInteropService.ClientCultureInfoUpdate -= HandleClientCultureInfoUpdate;
            Language = culture.TwoLetterISOLanguageName;
        }
    }

    private async void RPGPlayer_Disconnected(Player sender, PlayerQuitEventArgs e)
    {
        await LogOut();
        _cancellationTokenSource.Cancel();
        _logger.Verbose("Disconnected");
    }

    private void RPGPlayer_ResourceStarted(Player sender, PlayerResourceStartedEventArgs e)
    {
        ResourceStartingLatch.Decrement();
    }

    [ScriptMember("spawn")]
    public virtual async Task<bool> Spawn(Spawn spawn)
    {
        if(await spawn.IsAuthorized(this))
        {
            Camera.Target = this;
            Camera.Fade(CameraFade.In);
            Spawn(spawn.Position, spawn.Rotation.Z, 0, 0, 0);
            using var playerSpawnedEvent = new PlayerSpawnedEvent(this, spawn);
            await _eventFunctions.InvokeEvent(playerSpawnedEvent);
            _logger.Verbose("Spawned at {spawn}", spawn);
            return true;
        }
        return false;
    }

    [ScriptMember("isPersistant")]
    public bool IsPersistant() => true;

    // TODO: improve
    [ScriptMember("triggerClientEvent")]
    public void TriggerClientEvent(string name, params object[] values)
    {
        LuaValue luaValue;
        if (values.Length == 1 && values[0].GetType() == typeof(object[]))
        {
            luaValue = ((object[])values[0]).Select(_luaValueMapper.Map).ToArray();
        }
        else
        {
            luaValue = values.Select(_luaValueMapper.Map).ToArray();
        }

        if(values.Any())
            _logger.Verbose("Triggered client event {eventName} with arguments: {luaValue}.", name, luaValue);
        else
            _logger.Verbose("Triggered client event {eventName} with no arguments.", name);
        TriggerLuaEvent(name, this, luaValue);
    }

    [ScriptMember("logIn")]
    public async Task<bool> LogIn(PlayerAccount account, string password)
    {
        if (IsLoggedIn)
            return false;

        if (!await account.CheckPasswordAsync(password))
            return false;

        if (!_accountsInUseService.AssignPlayerToAccountId(this, account.Id))
            return false;

        await account.SignIn(Client.IPAddress?.ToString(), Client.Serial);

        Account = account;
        using var playerLoggedInEvent = new PlayerLoggedInEvent(this, account);
        await _eventFunctions.InvokeEvent(playerLoggedInEvent);
        LoggedIn?.Invoke(this, Account.Id);
        _logger.Verbose("Logged in to the account: {account}", account);
        return true;
    }

    [ScriptMember("logOut")]
    public async Task<bool> LogOut()
    {
        if (!IsLoggedIn || Account == null)
            return false;

        await Account.Save();
        using var playerLoggedOutEvent = new PlayerLoggedOutEvent(this);
        await _eventFunctions.InvokeEvent(playerLoggedOutEvent);
        var account = Account;
        Account = null;
        LoggedOut?.Invoke(this, account.Id);
        _logger.Verbose("Logged out account: {account}", account);
        return true;
    }

    [ScriptMember("openGui")]
    public bool OpenGui(string gui)
    {
        var success = _agnosticGuiSystemService.OpenGui(this, gui);
        if(success)
            _logger.Verbose("Opened gui {gui}", gui);
        else
            _logger.Verbose("Failed to open gui {gui}", gui);
        return success;
    }

    [ScriptMember("closeGui")]
    public bool CloseGui(string gui)
    {
        var success =_agnosticGuiSystemService.CloseGui(this, gui);
        if (success)
            _logger.Verbose("Closed gui {gui}", gui);
        else
            _logger.Verbose("Failed to close gui {gui}", gui);
        return success;
    }

    [ScriptMember("closeAllGuis")]
    public void CloseAllGuis()
    {
        _agnosticGuiSystemService.CloseAllGuis(this);
        _logger.Verbose("Closed all guis");
    }

    [ScriptMember("sendChatMessage")]
    public void SendChatMessage(string message, Color? color = null, bool isColorCoded = false)
    {
        _chatBox.Output(message, color, isColorCoded);
    }

    [ScriptMember("clearChatBox")]
    public void ClearChatBox()
    {
        _chatBox.ClearFor(this);
    }

    [ScriptMember("setClipboard")]
    public void SetClipboard(string content)
    {
        _luaInteropService.SetClipboard(this, content);
    }

    public void StartSession(SessionBase sessionBase)
    {
        _runningSessions.Add(sessionBase);
    }

    public bool IsDuringSession<T>() where T: SessionBase
    {
        return _runningSessions.OfType<T>().Any();
    }
    
    public T? GetRunningSession<T>() where T: SessionBase
    {
        return _runningSessions.OfType<T>().FirstOrDefault();
    }

    public void Reset()
    {
        Camera.Fade(CameraFade.Out, 0, Color.Black);
        Camera.Target = null;
        Account = null;
        ResourceStartingLatch = new(3); // TODO: remove hardcoded resources counter
        DebugView = false;
        _runningSessions.Clear();
    }

    [ScriptMember("longUserFriendlyName")]
    public string LongUserFriendlyName() => Name;
    [ScriptMember("toString")]
    public override string ToString() => Name;
}
