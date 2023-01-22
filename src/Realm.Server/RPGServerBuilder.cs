namespace Realm.Server;

public class RPGServerBuilder
{
    private readonly List<IModule> _modules = new();
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
            throw new Exception("Logger not configured.");
        if (_console == null)
            throw new Exception("Console not configured.");
        if (_realmConfigurationProvider == null)
            throw new Exception("RealmConfigurationProvider not configured.");

        return new RPGServer(_realmConfigurationProvider, _modules, serverBuilder =>
        {
            extraBuilderSteps?.Invoke(serverBuilder);

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
