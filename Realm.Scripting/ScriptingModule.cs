namespace Realm.Scripting;

public class ScriptingModule : IModule
{
    public string Name => "Scripting";
    private IScriptingModuleInterface? _interface;

    public void Configure(IServiceCollection services)
    {
        services.AddSingleton<EventFunctions>();
        services.AddSingleton<ModulesFunctions>();
        services.AddSingleton<IScriptingModuleInterface, JavascriptRuntime>();
    }

    public void Init(IServiceProvider serviceProvider)
    {
        _interface = serviceProvider.GetRequiredService<IScriptingModuleInterface>();
    }

    public void PostInit(IServiceProvider serviceProvider)
    {
        if (_interface == null)
            throw new InvalidOperationException();

        var path = serviceProvider.GetRequiredService<Func<string>>()();
        var logger = serviceProvider.GetRequiredService<ILogger>().ForContext<ScriptingModule>();
        var fileName = Path.Join(path, "Server/startup.js");

        logger.Information("Initializing startup.js: {fileName}", fileName);
        try
        {
            var code = File.ReadAllText(fileName);
            _interface.Execute(code, fileName);

            var typescriptDefinitions = _interface.GetTypescriptDefinition();
            var directory = Path.GetDirectoryName(fileName);
            File.WriteAllText(Path.Join(directory, "types.ts"), typescriptDefinitions);
            logger.Information("Scripting initialized, created types.js");
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Failed to initialize scripting!");
        }
    }

    public T GetInterface<T>() where T : class
    {
        if (_interface == null)
            throw new InvalidOperationException();

        if (typeof(T) == typeof(IScriptingModuleInterface))
        {
            var @interface = _interface as T;
            if(@interface == null)
                throw new InvalidOperationException();
            return @interface;
        }
        throw new ArgumentException();
    }

    public void Reload()
    {
    }

    public int GetPriority() => -100;
}
