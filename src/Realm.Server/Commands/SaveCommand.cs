namespace Realm.Server.Commands;

internal class SaveCommand : ICommand
{
    private readonly ECS _ecs;
    private readonly RealmDbContextFactory _realmDbContextFactory;
    private readonly ILoadAndSaveService _loadAndSaveService;

    public string CommandName => "save";

    public SaveCommand(ECS ecs, RealmDbContextFactory realmDbContextFactory, ILoadAndSaveService loadAndSaveService)
    {
        _ecs = ecs;
        _realmDbContextFactory = realmDbContextFactory;
        _loadAndSaveService = loadAndSaveService;
    }

    public async Task HandleCommand(string command)
    {
        int savedEntities = 0;
        using var context = _realmDbContextFactory.CreateDbContext();
        foreach (var entity in _ecs.Entities)
        {
            try
            {
                await _loadAndSaveService.Save(entity, context);
                savedEntities++;
            }
            catch (Exception ex)
            {
                ;
            }
        }
        await context.SaveChangesAsync();
    }
}
