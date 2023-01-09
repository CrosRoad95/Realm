namespace Realm.Server;

public class RPGServerBuilder
{
    private readonly List<IModule> _modules = new();
    private readonly List<Action<MtaServer>> _buildSteps = new();
    private ILogger? _logger;
    private IConsole? _console;
    private RealmConfigurationProvider? _realmConfigurationProvider;

    public RPGServerBuilder AddLogger(ILogger logger)
    {
        _logger = logger;
        return this;
    }

    public RPGServerBuilder AddConsole(IConsole console)
    {
        _console = console;
        return this;
    }

    public RPGServerBuilder AddConfiguration(RealmConfigurationProvider realmConfigurationProvider)
    {
        _realmConfigurationProvider = realmConfigurationProvider;
        return this;
    }

    public RPGServerBuilder AddModule<TModule>() where TModule : IModule, new()
    {
        _modules.Add(new TModule());
        return this;
    }

    public IRPGServer Build(IServerFilesProvider? serverFilesProvider = null, string? basePath = null, Action<ServerBuilder>? extraBuilderSteps = null)
    {
        if (_logger == null)
            throw new Exception();
        if (_console == null)
            throw new Exception();
        if (_realmConfigurationProvider == null)
            throw new Exception();

        return new RPGServer(_realmConfigurationProvider, _modules, serverBuilder =>
        {
            if (extraBuilderSteps != null)
                extraBuilderSteps(serverBuilder);
            serverBuilder.ConfigureServices(services =>
            {
                services.AddSingleton(_logger);
                services.AddSingleton(_console);
#if DEBUG
                serverFilesProvider ??= new ServerFilesProvider(basePath ?? "../../../Server");
#else
                serverFilesProvider ??= new ServerFilesProvider(basePath ?? "Server");
#endif
                services.AddSingleton(serverFilesProvider);
            });
        });
    }
}
