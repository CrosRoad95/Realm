using RealmCore.Interfaces.Extend;

namespace RealmCore.Server.Logic;

public sealed class DefaultModulesLogic
{
    public DefaultModulesLogic(IEnumerable<IModule> modules)
    {
        modules.ToList();
    }
}
