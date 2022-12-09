using Realm.Configuration;

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

    public RPGServer Build()
    {
        if (_logger == null)
            throw new Exception();
        if (_console == null)
            throw new Exception();
        if (_realmConfigurationProvider == null)
            throw new Exception();

        _logger.Information("Startin server:");
        _logger.Information("Modules: {modules}", string.Join(", ", _modules.Select(x => x.Name)));
        return new RPGServer(_realmConfigurationProvider, _modules, serverBuilder =>
        {
            serverBuilder.ConfigureServices(services =>
            {
                services.AddSingleton(_logger);
                services.AddSingleton(_console);
            });
        });
    }
}
