using RealmCore.Server.Components.Players.Abstractions;

namespace RealmCore.Server.Logic;

internal class GuiLogic
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IElementCollection _elementCollection;

    public GuiLogic(IServiceProvider serviceProvider, IElementCollection elementCollection, IGuiSystemService? guiSystemService = null)
    {
        _serviceProvider = serviceProvider;
        _elementCollection = elementCollection;
        if (guiSystemService != null)
            guiSystemService.GuiFilesChanged = HandleGuiFilesChanged;
    }

    private Task HandleGuiFilesChanged()
    {
        foreach (var player in _elementCollection.GetByType<RealmPlayer>())
        {
            var guiComponents = player.Components.ComponentsList.OfType<GuiComponent>().ToList();
            foreach (var guiComponent in guiComponents)
            {
                player.Components.DetachComponent(guiComponent);
                player.AddComponent(guiComponent);
            }
        }
        return Task.CompletedTask;
    }
}
