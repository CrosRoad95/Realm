namespace RealmCore.Server.Modules.Players.Gui;

internal sealed class GuiLogic
{
    private readonly IElementCollection _elementCollection;

    public GuiLogic(IElementCollection elementCollection, IGuiSystemService? guiSystemService = null)
    {
        _elementCollection = elementCollection;
        if (guiSystemService != null)
            guiSystemService.GuiFilesChanged = HandleGuiFilesChanged;
    }

    private Task HandleGuiFilesChanged()
    {
        foreach (var player in _elementCollection.GetByType<RealmPlayer>())
        {
            var gui = player.Gui.Current;
            player.Gui.Current = null;
            player.Gui.Current = gui;
        }
        return Task.CompletedTask;
    }
}
