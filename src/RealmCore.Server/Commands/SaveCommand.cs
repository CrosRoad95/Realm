namespace RealmCore.Server.Commands;

[CommandName("save")]
internal class SaveCommand : ICommand
{
    private readonly IEntityEngine _entityEngine;
    private readonly ISaveService _saveService;
    private readonly ILogger<SaveCommand> _logger;

    public SaveCommand(IEntityEngine entityEngine, ISaveService saveService, ILogger<SaveCommand> logger)
    {
        _entityEngine = entityEngine;
        _saveService = saveService;
        _logger = logger;
    }

    public async Task Handle(Entity consoleEntity, CommandArguments args)
    {
        int savedEntities = 0;
        foreach (var entity in _entityEngine.Entities)
        {
            using var _ = _logger.BeginEntity(entity);
            try
            {
#if DEBUG
                if (await _saveService.BeginSave(entity))
                {
                    await _saveService.Commit();
                    savedEntities++;
                }
#else
                if (await _saveService.BeginSave(entity))
                    savedEntities++;
#endif
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save entity: {entityName}", entity.ToString());
            }
        }
#if !DEBUG
        await _saveService.Commit();
#endif
    }
}
