namespace Realm.Scripting.Classes;

[NoDefaultScriptAccess]
public class ModulesScriptingFunctions
{
    private readonly List<IModule> _modules;

    public ModulesScriptingFunctions(IEnumerable<IModule> modules)
    {
        _modules = new List<IModule>(modules);
    }

    [ScriptMember("getModules")]
    public object GetModules()
    {
        return _modules.Select(x => x.Name).ToArray().ToScriptArray();
    }

    [ScriptMember("moduleExists")]
    public bool ModuleExists(string module) => _modules.Any(x => x.Name == module);

    [ScriptMember("toString")]
    public override string ToString() => "Module";
}
