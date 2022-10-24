namespace Realm.Scripting.Classes;

public class ModulesFunctions
{
    private readonly List<IModule> _modules;

    public ModulesFunctions(IEnumerable<IModule> modules)
    {
        _modules = new List<IModule>(modules);
    }

    public object GetModules()
    {
        return _modules.Select(x => x.Name).ToArray().ToScriptArray();
    }

    public bool ModuleExists(string module) => _modules.Any(x => x.Name == module);

    public override string ToString() => "Module";
}
