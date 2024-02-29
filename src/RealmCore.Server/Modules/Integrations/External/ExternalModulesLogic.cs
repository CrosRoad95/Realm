namespace RealmCore.Server.Modules.Integrations.External;

public sealed class ExternalModulesLogic
{
    private readonly List<IExternalModule> _modules;

    public ExternalModulesLogic(IEnumerable<IExternalModule> modules)
    {
        _modules = modules.ToList();
    }
}
