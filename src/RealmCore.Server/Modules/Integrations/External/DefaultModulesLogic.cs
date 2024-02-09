namespace RealmCore.Server.Modules.Integrations.External;

public sealed class DefaultModulesLogic
{
    private readonly List<IModule> _modules;

    public DefaultModulesLogic(IEnumerable<IModule> modules)
    {
        _modules = modules.ToList();
    }
}
