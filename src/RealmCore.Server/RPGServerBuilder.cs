namespace RealmCore.Server;

public class RPGServerBuilder
{
    private Serilog.ILogger? _logger;
    private IConsole? _console;
    private IRealmConfigurationProvider? _realmConfigurationProvider;

    public RPGServerBuilder AddLogger(Serilog.ILogger logger)
    {
        _logger = logger;
        return this;
    }

    public RPGServerBuilder AddConsole(IConsole console)
    {
        _console = console;
        return this;
    }

    public RPGServerBuilder AddConfiguration(IRealmConfigurationProvider realmConfigurationProvider)
    {
        _realmConfigurationProvider = realmConfigurationProvider;
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

        return new RPGServer(_realmConfigurationProvider, serverBuilder =>
        {
            extraBuilderSteps?.Invoke(serverBuilder);

            serverBuilder.ConfigureServices(services =>
            {
                services.AddLogging(x => x.AddSerilog(_logger, dispose: true));
                services.AddTransient<ILogger>(x => x.GetRequiredService<ILogger<MtaServer>>());
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
