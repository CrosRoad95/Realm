using Realm.Interfaces.Extend;

namespace Realm.Server.Logic;

public sealed class DefaultModulesLogic
{
    public DefaultModulesLogic(IEnumerable<IModule> modules)
    {
        modules.ToList();
    }
}
