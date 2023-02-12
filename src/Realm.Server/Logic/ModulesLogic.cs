using Realm.Interfaces.Extend;

namespace Realm.Server.Logic;

internal sealed class ModulesLogic
{
    public ModulesLogic(IEnumerable<IModule> modules)
    {
        modules.ToList();
    }
}
