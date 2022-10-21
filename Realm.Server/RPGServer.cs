using Realm.Scripting.Classes;
using Realm.Scripting.Interfaces;
using Realm.Server.Scripting;
using Realm.Server.Scripting.Events;

namespace Realm.Server;

public partial class RPGServer : IRPGServer
{
    private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(0);
    private readonly MtaServer<RPGPlayer> _server;
    private readonly SlipeServerConfiguration _serverConfiguration;
    private readonly ScriptingConfiguration _scriptingConfiguration;
    private readonly ILogger _logger;
    private readonly LuaEventContextFactory _luaEventContextFactory;
    private readonly EventFunctions _eventFunctions;
    private readonly ElementFunctions _elementFunctions;
    private readonly IEnumerable<IModule> _modules;

    public event Action<Player>? PlayerJoined;

    public RPGServer(ConfigurationProvider configurationProvider, ILogger logger, IEnumerable<IModule> modules, Action<ServerBuilder>? configureServerBuilder = null)
    {
        _modules = modules;
        _logger = logger.ForContext<IRPGServer>();
        _serverConfiguration = configurationProvider.Get<SlipeServerConfiguration>("server");
        _scriptingConfiguration = configurationProvider.Get<ScriptingConfiguration>("scripting");
        _server = MtaServer.CreateWithDiSupport<RPGPlayer>(
            builder =>
            {
                builder.ConfigureServer(configurationProvider.Configuration);
                if (configureServerBuilder != null)
                    configureServerBuilder(builder);

                builder.ConfigureServices(services =>
                {
                    services.AddSingleton(configurationProvider);
                    services.AddSingleton(logger);
                    services.AddSingleton(this);
                    services.AddSingleton<IRPGServer>(this);
                    services.AddSingleton<LuaEventContextFactory>();
                    services.AddSingleton<ElementFunctions>();

                    if (modules != null)
                        foreach (var module in modules)
                        {
                            services.AddSingleton(module);
                            module.Configure(services);
                        }
                });
            }
        );


        var serverListConfiguration = configurationProvider.Get<ServerListConfiguration>("serverList");
        _server.GameType = serverListConfiguration.GameType;
        _server.MapName = serverListConfiguration.MapName;
        _server.PlayerJoined += e => PlayerJoined?.Invoke(e);
        _server.LuaEventTriggered += Server_LuaEventTriggered;

        var startup = _server.GetRequiredService<Startup>();
        _luaEventContextFactory = _server.GetRequiredService<LuaEventContextFactory>();
        _eventFunctions = _server.GetRequiredService<EventFunctions>();
        _elementFunctions = _server.GetRequiredService<ElementFunctions>();

        var _ = Task.Run(startup.StartAsync);

        Console.CancelKeyPress += (sender, args) =>
        {
            _server.Stop();
            _semaphore.Release();
        };

        _server.PlayerJoined += Server_PlayerJoined;
    }

    public void AssociateElement(Element element)
    {
        _server.AssociateElement(element);
    }

    private async void Server_PlayerJoined(RPGPlayer player)
    {
        await _eventFunctions.InvokeEvent("onPlayerJoin", new PlayerJoinedEvent
        {
            Player = player,
        });
    }

    public void InitializeScripting(IScriptingModuleInterface scriptingModuleInterface)
    {
        // Events
        _eventFunctions.RegisterEvent("onPlayerJoin");

        // Functions
        scriptingModuleInterface.AddHostObject("Elements", _elementFunctions, true);

        // Classes & Events & Contextes
        scriptingModuleInterface.AddHostType(typeof(RPGPlayer));
        scriptingModuleInterface.AddHostType(typeof(Spawn));
    }

    private async void Server_LuaEventTriggered(LuaEvent luaEvent)
    {

    }

    public TService GetRequiredService<TService>() where TService: notnull
    {
        return _server.GetRequiredService<TService>();
    }

    public async Task Start()
    {
        _logger.Information("Initializing modules: {modules}", string.Join(", ", _modules.Select(x => x.Name)));
        var serviceProvider = _server.GetRequiredService<IServiceProvider>();
        foreach (var module in _modules)
            module.Init(serviceProvider);

        var scriptingModule = _modules.FirstOrDefault(x => x.Name == "Scripting");
        if (scriptingModule != null)
            InitializeScripting(scriptingModule.GetInterface<IScriptingModuleInterface>());

        foreach (var module in _modules)
            module.PostInit(serviceProvider);

        _logger.Information("Server started at port: {port}", _serverConfiguration.Port);
        _server.Start();
        await _semaphore.WaitAsync();
    }
}