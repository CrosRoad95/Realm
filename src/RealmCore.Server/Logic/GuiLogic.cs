using RealmCore.Server.Components.Players.Abstractions;

namespace RealmCore.Server.Logic;

internal class GuiLogic
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IEntityEngine _entityEngine;

    public GuiLogic(IServiceProvider serviceProvider, IRealmServer realmServer, IEntityEngine entityEngine)
    {
        _serviceProvider = serviceProvider;
        _entityEngine = entityEngine;
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
        foreach (var entity in _entityEngine.PlayerEntities)
        {
            var guiComponents = entity.Components.OfType<GuiComponent>().ToList();
            foreach (var guiComponent in guiComponents)
            {
                entity.DetachComponent(guiComponent);
                entity.AddComponent(guiComponent);
            }
        }
        return Task.CompletedTask;
    }
}
