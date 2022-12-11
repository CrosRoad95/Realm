namespace Realm.Server.Modules;

public class ServerScriptingModule : IModule
{
    public string Name => "Server";

    public void Configure(IServiceCollection services)
    {
    }

    public T GetInterface<T>() where T : class
    {
        throw new NotSupportedException();
    }

    public int GetPriority() => 50;

    public void Init(IServiceProvider serviceProvider)
    {
    }

    public void PostInit(IServiceProvider serviceProvider)
    {
        var discordIntegration = serviceProvider.GetRequiredService<IRPGServer>();

        var scriptingModule = serviceProvider.GetRequiredService<IEnumerable<IModule>>().FirstOrDefault(x => x.Name == "Scripting");
        if (scriptingModule != null)
            discordIntegration.InitializeScripting(scriptingModule.GetInterface<IScriptingModuleInterface>());
    }

    public void Reload()
    {
    }
}
