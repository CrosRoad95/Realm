using RealmCore.ECS;

namespace RealmCore.Server.Commands;

[CommandName("reloadentities")]
internal class ReloadEntitiesCommand : ICommand
{
    private readonly IEntityEngine _ecs;
    private readonly ISaveService _saveService;
    private readonly ILogger<SaveCommand> _logger;
    private readonly ILoadService _loadService;

    public ReloadEntitiesCommand(IEntityEngine ecs, ISaveService saveService, ILogger<SaveCommand> logger, ILoadService loadService)
    {
        _ecs = ecs;
        _saveService = saveService;
        _logger = logger;
        _loadService = loadService;
    }

    public async Task Handle(Entity consoleEntity, CommandArguments args)
    {
        int savedEntities = 0;
        foreach (var entity in _ecs.Entities)
        {
            try
            {
                if (await _saveService.Save(entity))
                {
#if DEBUG
                    await _saveService.Commit();
#endif
                    savedEntities++;
                    entity.Dispose();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save entity: {entityName}", entity.ToString());
            }
        }
#if !DEBUG
        await _saveService.Commit();
#endif
        await _loadService.LoadAll();
    }
}
