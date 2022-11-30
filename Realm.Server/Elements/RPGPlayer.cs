using Realm.Server.Concepts.Inventory;

namespace Realm.Server.Elements;

[NoDefaultScriptAccess]
public class RPGPlayer : Player
{
    private const int _RESOURCE_COUNT = 8;
    private readonly CancellationTokenSource _cancellationTokenSource;
    private readonly AgnosticGuiSystemService _agnosticGuiSystemService;
    private readonly AccountsInUseService _accountsInUseService;
    private readonly LuaInteropService _luaInteropService;
    private readonly MtaServer _mtaServer;
    private readonly List<SessionBase> _runningSessions = new();
    public ComponentSystem? _componentsSystem;
    public InventorySystem? _inventorySystem;
    public Latch ResourceStartingLatch = new(_RESOURCE_COUNT); // TODO: remove hardcoded resources counter
    public CancellationToken CancellationToken { get; private set; }

    public MtaServer MtaServer => _mtaServer;

    [ScriptMember("account", ScriptAccess.ReadOnly)]
    public PlayerAccount? Account { get; private set; }
    [ScriptMember("language", ScriptAccess.ReadOnly)]
    public string Language { get; private set; } = "pl";
    [ScriptMember("isLoggedIn", ScriptAccess.ReadOnly)]
    public bool IsLoggedIn => Account != null && Account.IsAuthenticated;
    [ScriptMember("components")]
    public ComponentSystem Components
    {
        get
        {
            if(_componentsSystem != null)
                return _componentsSystem;
            throw new Exception();
        }
    }
    [ScriptMember("inventory")]
    public InventorySystem Inventory
    {
        get
        {
            if (_inventorySystem != null)
                return _inventorySystem;
            throw new Exception();
        }
    }

    private bool _debugView = false;
    [ScriptMember("debugView")]
    public bool DebugView { get => _debugView; set
        {
            if(_debugView != value)
            {
                _debugView = value;
                DebugViewStateChanged?.Invoke(this, value);
            }
        }
    }

    private bool _adminTools = false;
    [ScriptMember("adminTools")]
    public bool AdminTools
    {
        get => _adminTools; set
        {
            if(_adminTools != value)
            {
                _adminTools = value;
                AdminToolsStateChanged?.Invoke(this, value);
            }
        }
    }

    private bool _noClip = false;
    [ScriptMember("noClip")]
    public bool NoClip
    {
        get => _noClip; set
        {
            if(_noClip != value)
            {
                _noClip = value;
                NoClipStateChanged?.Invoke(this, value);
            }
        }
    }

    public event Action<RPGPlayer, bool>? AdminToolsStateChanged;
    public event Action<RPGPlayer, bool>? NoClipStateChanged;
    public new event Action<RPGPlayer, RPGSpawn>? Spawned;
    public event Action<RPGPlayer, Vector3>? SpawnedAtPosition;
    public event Action<RPGPlayer, PlayerAccount>? LoggedIn;
    public event Action<RPGPlayer, string>? LoggedOut;
    public event Action<RPGPlayer, string>? ClipboardChanged;
    public event Action<RPGPlayer, bool>? DebugViewStateChanged;
    public event Action<RPGPlayer, string, object[]>? EventTriggered;
    public event Action<RPGPlayer, string, Color?, bool?>? ChatMessageSend;
    public event Action<RPGPlayer>? ClearChatRequested;
    public event Action<RPGPlayer, string>? GuiOpened;
    public event Action<RPGPlayer, string>? GuiClosed;
    public event Action<RPGPlayer>? AllGuiClosed;

    public RPGPlayer(AgnosticGuiSystemService agnosticGuiSystemService,
        AccountsInUseService accountsInUseService, LuaInteropService luaInteropService,
        MtaServer mtaServer)
    {
        _agnosticGuiSystemService = agnosticGuiSystemService;
        _accountsInUseService = accountsInUseService;
        _luaInteropService = luaInteropService;
        _mtaServer = mtaServer;
        _cancellationTokenSource = new CancellationTokenSource();
        CancellationToken = _cancellationTokenSource.Token;
        ResourceStarted += RPGPlayer_ResourceStarted;

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

    private void RPGPlayer_ResourceStarted(Player sender, PlayerResourceStartedEventArgs e)
    {
        ResourceStartingLatch.Decrement();
    }

    [ScriptMember("spawn")]
    public async Task<bool> Spawn(RPGSpawn spawn)
    {
        if(await spawn.IsAuthorized(this))
        {
            Camera.Target = this;
            Camera.Fade(CameraFade.In);
            Spawn(spawn.Position, spawn.Rotation.Z, 0, 0, 0);
            Spawned?.Invoke(this, spawn);
            return true;
        }
        return false;
    }
    
    [ScriptMember("spawn")]
    public void Spawn(Vector3 position, Vector3? rotation = null)
    {
        Camera.Target = this;
        Camera.Fade(CameraFade.In);
        Spawn(position, rotation?.Z ?? 0, 0, 0, 0);
        SpawnedAtPosition?.Invoke(this, position);
    }

    [ScriptMember("isPersistant")]
    public bool IsPersistant() => true;

    [ScriptMember("triggerClientEvent")]
    public void TriggerClientEvent(string name, params object[] values)
    {
        EventTriggered?.Invoke(this, name, values);
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

        if (!string.IsNullOrEmpty(account.ComponentsData))
            _componentsSystem = ComponentSystem.CreateFromString(account.ComponentsData);
        else
            _componentsSystem = new ComponentSystem();

        if (!string.IsNullOrEmpty(account.InventoryData))
            _inventorySystem = InventorySystem.CreateFromString(account.InventoryData);
        else
            _inventorySystem = new InventorySystem();

        _inventorySystem.SetOwner(this);
        _componentsSystem.SetOwner(this);

        LoggedIn?.Invoke(this, Account);
        return true;
    }

    [ScriptMember("logOut")]
    public async Task<bool> LogOut()
    {
        if (!IsLoggedIn || Account == null)
            return false;

        await Save();
        LoggedOut?.Invoke(this, Account.Id);
        _cancellationTokenSource.Cancel();
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
    public void OpenGui(string guiName)
    {
        GuiOpened?.Invoke(this, guiName);
    }

    [ScriptMember("closeGui")]
    public void CloseGui(string gui)
    {
        GuiClosed?.Invoke(this, gui);
    }

    [ScriptMember("closeAllGuis")]
    public void CloseAllGuis()
    {
        AllGuiClosed?.Invoke(this);
    }

    [ScriptMember("sendChatMessage")]
    public void SendChatMessage(string message, Color? color = null, bool isColorCoded = false)
    {
        ChatMessageSend?.Invoke(this, message, color, isColorCoded);
    }

    [ScriptMember("clearChatBox")]
    public void ClearChatBox()
    {
        ClearChatRequested?.Invoke(this);
    }

    [ScriptMember("setClipboard")]
    public void SetClipboard(string content)
    {
        ClipboardChanged?.Invoke(this, content);
    }

    public void AddSession(SessionBase sessionBase)
    {
        _runningSessions.Add(sessionBase);
    }

    public void RemoveSession(SessionBase sessionBase)
    {
        _runningSessions.Remove(sessionBase);
    }

    public bool IsDuringSession<T>() where T: SessionBase
    {
        return _runningSessions.OfType<T>().Any();
    }
    
    public T GetRequiredSession<T>() where T: SessionBase
    {
        return _runningSessions.OfType<T>().First();
    }

    [ScriptMember("getSession", ScriptMemberFlags.ExposeRuntimeType)]
    public SessionBase? GetSession(Type type)
    {
        return _runningSessions.Where(x => x.GetType() == type).FirstOrDefault();
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
