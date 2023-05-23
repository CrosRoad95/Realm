using SlipeServer.Server.Elements;

namespace RealmCore.Resources.CEFBlazorGui;

internal sealed class CEFBlazorGuiService : ICEFBlazorGuiService
{
    public event Action<Player>? PlayerCEFBlazorGuiStarted;
    public event Action<Player>? PlayerCEFBlazorGuiStopped;
    private readonly HashSet<Player> _CEFBlazorGuiPlayers = new();

    public CEFBlazorGuiService()
    {
    }

    public bool IsCEFBlazorGui(Player player) => _CEFBlazorGuiPlayers.Contains(player);

    public void HandleCEFBlazorGuiStart(Player player)
    {
        if (_CEFBlazorGuiPlayers.Add(player))
            PlayerCEFBlazorGuiStarted?.Invoke(player);
    }

    public void HandleCEFBlazorGuiStop(Player player)
    {
        if (_CEFBlazorGuiPlayers.Remove(player))
            PlayerCEFBlazorGuiStopped?.Invoke(player);
    }
}
