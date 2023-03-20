namespace Realm.Server.Commands;

[CommandName("save")]
internal class SaveCommand : ICommand
{
    private readonly ECS _ecs;
    private readonly ISaveService _saveService;
    private readonly ILogger<SaveCommand> _logger;

    public SaveCommand(ECS ecs, ISaveService saveService, ILogger<SaveCommand> logger)
    {
        _ecs = ecs;
        _saveService = saveService;
        _logger = logger;
    }

    public async Task HandleCommand(string command)
    {
        int savedEntities = 0;
        foreach (var entity in _ecs.Entities)
        {
            try
            {
                await _saveService.Save(entity);
#if DEBUG
                await _saveService.Commit();
#endif
                savedEntities++;
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
