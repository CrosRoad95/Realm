using Realm.Module.Scripting.Functions;
using Realm.Module.Scripting.Interfaces;

namespace Realm.Module.Scripting;

public class ScriptingModule : IModule
{
    public string Name => "Scripting";
    private IScriptingModuleInterface? _interface;

    public void Configure(IServiceCollection services)
    {
        services.AddSingleton<EventScriptingFunctions>();
        services.AddSingleton<ModulesScriptingFunctions>();
        services.AddSingleton<UlitityScriptingFunctions>();
        services.AddSingleton<JavascriptRuntime>();
        services.AddSingleton<IScriptingModuleInterface>(x => x.GetRequiredService<JavascriptRuntime>());
        services.AddSingleton<IReloadable>(x => x.GetRequiredService<JavascriptRuntime>());
    }

    public void Init(IServiceProvider serviceProvider)
    {
        _interface = serviceProvider.GetRequiredService<IScriptingModuleInterface>();
    }

    public void PostInit(IServiceProvider serviceProvider)
    {
        if (_interface == null)
            throw new InvalidOperationException();

        var logger = serviceProvider.GetRequiredService<ILogger>().ForContext<ScriptingModule>();

        try
        {
            _interface.Start();
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
            if (@interface == null)
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
