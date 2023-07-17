namespace RealmCore.Server.Commands;

[CommandName("save")]
internal class SaveCommand : ICommand
{
    private readonly IECS _ecs;
    private readonly ISaveService _saveService;
    private readonly ILogger<SaveCommand> _logger;

    public SaveCommand(IECS ecs, ISaveService saveService, ILogger<SaveCommand> logger)
    {
        _ecs = ecs;
        _saveService = saveService;
        _logger = logger;
    }

    public async Task Handle(Entity consoleEntity, CommandArguments args)
    {
        int savedEntities = 0;
        foreach (var entity in _ecs.Entities)
        {
            using var _ = _logger.BeginEntity(entity);
            try
            {
#if DEBUG
                if (await _saveService.Save(entity))
                {
                    await _saveService.Commit();
                    savedEntities++;
                }
#else
                if (await _saveService.Save(entity))
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
