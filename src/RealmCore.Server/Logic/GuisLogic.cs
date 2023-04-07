namespace RealmCore.Server.Logic;

internal class GuisLogic
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IECS _ecs;

    public GuisLogic(IServiceProvider serviceProvider, IRPGServer rpgServer, IECS ecs)
    {
        _serviceProvider = serviceProvider;
        _ecs = ecs;
        rpgServer.ServerStarted += HandleServerStarted;
    }

    private void HandleServerStarted()
    {
        _serviceProvider.GetRequiredService<IAgnosticGuiSystemService>().GuiFilesChanged = HandleGuiFilesChanged;
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
