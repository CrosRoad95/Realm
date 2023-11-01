using RealmCore.Server.Components.Players.Abstractions;

namespace RealmCore.Server.Logic;

internal class GuiLogic
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IElementCollection _elementCollection;

    public GuiLogic(IServiceProvider serviceProvider, RealmServer realmServer, IElementCollection elementCollection)
    {
        _serviceProvider = serviceProvider;
        _elementCollection = elementCollection;
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
        foreach (var player in _elementCollection.GetByType<RealmPlayer>())
        {
            var guiComponents = player.Components.ComponentsLists.OfType<GuiComponent>().ToList();
            foreach (var guiComponent in guiComponents)
            {
                player.Components.DetachComponent(guiComponent);
                player.AddComponent(guiComponent);
            }
        }
        return Task.CompletedTask;
    }
}
