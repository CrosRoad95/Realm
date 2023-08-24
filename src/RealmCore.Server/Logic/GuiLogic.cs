namespace RealmCore.Server.Logic;

internal class GuiLogic
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IECS _ecs;

    public GuiLogic(IServiceProvider serviceProvider, IRealmServer realmServer, IECS ecs)
    {
        _serviceProvider = serviceProvider;
        _ecs = ecs;
        realmServer.ServerStarted += HandleServerStarted;
    }

    private void HandleServerStarted()
    {
        var guiSystemProvider = _serviceProvider.GetService<IGuiSystemService>();
        if(guiSystemProvider != null)
            guiSystemProvider.GuiFilesChanged = HandleGuiFilesChanged;
    }

    private Task HandleGuiFilesChanged()
    {
        foreach (var entity in _ecs.PlayerEntities)
        {
            var guiComponents = entity.Components.OfType<GuiComponent>().ToList();
            foreach (var guiComponent in guiComponents)
            {
                guiComponent.Close();
                entity.DetachComponent(guiComponent);
                entity.AddComponent(guiComponent);
            }
        }
        return Task.CompletedTask;
    }
}
