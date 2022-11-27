namespace Realm.Server.Elements;

[NoDefaultScriptAccess]
public class RPGPlayer : Player
{
    private const int _RESOURCE_COUNT = 8;
    private readonly CancellationTokenSource _cancellationTokenSource;
    private readonly LuaValueMapper _luaValueMapper;
    private readonly DebugLog _debugLog;
    private readonly AgnosticGuiSystemService _agnosticGuiSystemService;
    private readonly AccountsInUseService _accountsInUseService;
    private readonly LuaInteropService _luaInteropService;
    private readonly ChatBox _chatBox;
    private readonly MtaServer _mtaServer;
    private readonly ILogger _logger;
    public Latch ResourceStartingLatch = new(_RESOURCE_COUNT); // TODO: remove hardcoded resources counter
    public CancellationToken CancellationToken { get; private set; }

    public MtaServer MtaServer => _mtaServer;

    [ScriptMember("account", ScriptAccess.ReadOnly)]
    public PlayerAccount? Account { get; private set; }
    [ScriptMember("language", ScriptAccess.ReadOnly)]
    public string Language { get; private set; } = "pl";
    [ScriptMember("isLoggedIn", ScriptAccess.ReadOnly)]
    public bool IsLoggedIn => Account != null && Account.IsAuthenticated;

    public event Action<RPGPlayer, bool>? AdminToolsStateChanged;
    public event Action<RPGPlayer, bool>? NoClipStateChanged;
    public new event Action<RPGPlayer, RPGSpawn>? Spawned;
    public event Action<RPGPlayer, PlayerAccount>? LoggedIn;
    public event Action<RPGPlayer, string>? LoggedOut;

    public event Action<RPGPlayer>? NotifyNotSavedState;

    [ScriptMember("components")]
    public ComponentSystem Components;
    [ScriptMember("inventory")]
    public InventorySystem Inventory;

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

    private bool _adminTools = false;
    [ScriptMember("adminTools")]
    public bool AdminTools
    {
        get => _adminTools; set
        {
            _adminTools = value;
            AdminToolsStateChanged?.Invoke(this, value);
            if (value)
                _logger.Verbose("Enabled admin tools");
            else
                _logger.Verbose("Disabled admin tools");
        }
    }

    private bool _noClip = false;
    [ScriptMember("noClip")]
    public bool NoClip
    {
        get => _noClip; set
        {
            _noClip = value;
            NoClipStateChanged?.Invoke(this, value);
            if (value)
                _logger.Verbose("Enabled no clip");
            else
                _logger.Verbose("Disabled no clip");
        }
    }

    private readonly List<SessionBase> _runningSessions = new();

    public RPGPlayer(LuaValueMapper luaValueMapper, DebugLog debugLog, AgnosticGuiSystemService agnosticGuiSystemService,
        AccountsInUseService accountsInUseService, ILogger logger, LuaInteropService luaInteropService, ChatBox chatBox,
        MtaServer mtaServer)
    {
        _luaValueMapper = luaValueMapper;
        _debugLog = debugLog;
        _agnosticGuiSystemService = agnosticGuiSystemService;
        _accountsInUseService = accountsInUseService;
        _luaInteropService = luaInteropService;
        _chatBox = chatBox;
        _mtaServer = mtaServer;
        _logger = logger
            .ForContext<RPGPlayer>()
            .ForContext(new RPGPlayerEnricher(this));
        _cancellationTokenSource = new CancellationTokenSource();
        CancellationToken = _cancellationTokenSource.Token;
        Components = new ComponentSystem(this, _logger);
        Inventory = new InventorySystem(this, _logger);
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
    public virtual async Task<bool> Spawn(RPGSpawn spawn)
    {
        if(await spawn.IsAuthorized(this))
        {
            Camera.Target = this;
            Camera.Fade(CameraFade.In);
            Spawn(spawn.Position, spawn.Rotation.Z, 0, 0, 0);
            Spawned?.Invoke(this, spawn);
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

        await account.SignIn(Client.IPAddress?.ToString(), Client.Serial!);

        Account = account;
        using var playerLoggedInEvent = new PlayerLoggedInEvent(this, account);
        LoggedIn?.Invoke(this, Account);

        if (!string.IsNullOrEmpty(account.ComponentsData))
        {
            Components = JsonConvert.DeserializeObject<ComponentSystem>(account.ComponentsData, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Objects,
            }) ?? throw new JsonSerializationException("Failed to deserialize Components");
            Components.SetLogger(_logger);
            Components.SetOwner(this);
            Components.AfterLoad();
        }
        
        if (!string.IsNullOrEmpty(account.InventoryData))
        {
            Inventory = JsonConvert.DeserializeObject<InventorySystem>(account.InventoryData, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Objects,
            }) ?? throw new JsonSerializationException("Failed to deserialize Inventory");
            Inventory.SetLogger(_logger);
            Inventory.SetOwner(this);
            Inventory.AfterLoad();
        }

        _logger.Verbose("Logged in to the account: {account}", account);
        return true;
    }

    [ScriptMember("logOut")]
    public async Task<bool> LogOut()
    {
        if (!IsLoggedIn || Account == null)
            return false;

        await Save();
        LoggedOut?.Invoke(this, Account.Id);
        _logger.Verbose("Logged out from account: {account}", Account);
        Account = null;
        return true;
    }

    public async Task Save()
    {
        if (!IsLoggedIn || Account == null)
            return;

        Account.ComponentsData = JsonConvert.SerializeObject(Components, Formatting.None, new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Objects,
        });
        Account.InventoryData = JsonConvert.SerializeObject(Inventory, Formatting.None, new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Objects,
        });
        await Account.Save();
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
        ResourceStartingLatch = new(_RESOURCE_COUNT); // TODO: remove hardcoded resources counter
        DebugView = false;
        _runningSessions.Clear();
    }

    [ScriptMember("toString")]
    public override string ToString() => Name;
}
