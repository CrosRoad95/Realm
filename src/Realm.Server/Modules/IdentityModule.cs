using Realm.Module.Scripting.Interfaces;

namespace Realm.Server.Modules;

public class IdentityModule : IModule
{
    public string Name => "Identity";

    public void Configure(IServiceCollection services)
    {
        services.AddSingleton<IdentityScriptingFunctions>();
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
        scriptingModuleInterface.AddHostType(typeof(PlayerAccount));
        scriptingModuleInterface.AddHostType(typeof(PlayerRole));
        scriptingModuleInterface.AddHostType(typeof(DiscordUser));
        scriptingModuleInterface.AddHostObject("Identity", serviceProvider.GetRequiredService<IdentityScriptingFunctions>(), true);
    }

    public int GetPriority() => 100;
}
