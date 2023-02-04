namespace Realm.Server.Logic;

internal class ModulesLogic
{
    public ModulesLogic(ILogger logger, IServiceProvider serviceProvider, IEnumerable<IModule> modules)
    {
        foreach (var module in modules)
            module.Init(serviceProvider);
        logger.LogInformation("Loaded modules: {modules}", string.Join(", ", modules.Select(x => x.Name)));
    }
}
