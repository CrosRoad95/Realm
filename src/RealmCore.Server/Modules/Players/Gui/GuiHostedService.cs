namespace RealmCore.Server.Modules.Players.Gui;

internal sealed class GuiHostedService : IHostedService
{
    private readonly IElementCollection _elementCollection;
    private readonly IGuiSystemService? _guiSystemService;

    public GuiHostedService(IElementCollection elementCollection, IGuiSystemService? guiSystemService = null)
    {
        _elementCollection = elementCollection;
        _guiSystemService = guiSystemService;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        if (_guiSystemService != null)
            _guiSystemService.GuiFilesChanged = HandleGuiFilesChanged;
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        if (_guiSystemService != null)
            _guiSystemService.GuiFilesChanged = null;
        return Task.CompletedTask;
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
