namespace Realm.Server.Commands;

internal class SaveCommand : ICommand
{
    private readonly ECS _ecs;
    private readonly ISaveService _saveService;

    public string CommandName => "save";

    public SaveCommand(ECS ecs, ISaveService saveService)
    {
        _ecs = ecs;
        _saveService = saveService;
    }

    public async Task HandleCommand(string command)
    {
        int savedEntities = 0;
        foreach (var entity in _ecs.Entities)
        {
            try
            {
                await _saveService.Save(entity);
                savedEntities++;
            }
            catch (Exception ex)
            {
                ;
            }
        }
        await _saveService.Commit();
    }
}
