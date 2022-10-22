namespace Realm.Persistance;

public class IdentityModule : IModule
{
    public string Name => "Identity-Scripting";

    public void Configure(IServiceCollection services)
    {
        services.AddSingleton<IdentityFunctions>();
    }

    public void Init(IServiceProvider serviceProvider)
    {
    }

    public T GetInterface<T>() where T : class => throw new NotSupportedException();

    public void PostInit(IServiceProvider serviceProvider)
    {
        var scriptingModule = serviceProvider.GetRequiredService<IEnumerable<IModule>>().FirstOrDefault(x => x.Name == "Scripting");
        if (scriptingModule != null)
            InitializeScripting(scriptingModule.GetInterface<IScriptingModuleInterface>(), serviceProvider);
    }

    private void InitializeScripting(IScriptingModuleInterface scriptingModuleInterface, IServiceProvider serviceProvider)
    {
        scriptingModuleInterface.AddHostType(typeof(User));
        scriptingModuleInterface.AddHostObject("Identity", serviceProvider.GetRequiredService<IdentityFunctions>(), true);
    }

    public void Reload()
    {

    }

    public int GetPriority() => 100;
}
