using SlipeServer.Server.Elements;
using System.Drawing;

namespace RealmCore.Resources.ElementOutline;

public interface IElementOutlineService
{
    event Action<Player, bool>? RenderingEnabled;
    internal event Action<Element, Color>? OutlineChanged;
    internal event Action<Element>? OutlineRemoved;
    internal event Action<Player, Element, Color>? OutlineForPlayerChanged;
    internal event Action<Player, Element>? OutlineForPlayerRemoved;

    void RemoveElementOutline(Element target);
    void RemoveElementOutlineForPlayer(Player player, Element target);
    void SetElementOutline(Element target, Color color);
    void SetElementOutlineForPlayer(Player player, Element target, Color color);
    void SetRenderingEnabled(Player player, bool enabled);
}

internal sealed class ElementOutlineService : IElementOutlineService
{
    public event Action<Player, Element, Color>? OutlineForPlayerChanged;
    public event Action<Player, Element>? OutlineForPlayerRemoved;

    public event Action<Element, Color>? OutlineChanged;
    public event Action<Element>? OutlineRemoved;
    public event Action<Player, bool>? RenderingEnabled;

    public void SetElementOutlineForPlayer(Player player, Element target, Color color)
    {
        OutlineForPlayerChanged?.Invoke(player, target, color);
    }

    public void RemoveElementOutlineForPlayer(Player player, Element target)
    {
        OutlineForPlayerRemoved?.Invoke(player, target);
    }

    public void SetElementOutline(Element target, Color color)
    {
        OutlineChanged?.Invoke(target, color);
    }

    public void RemoveElementOutline(Element target)
    {
        OutlineRemoved?.Invoke(target);
    }

    public void SetRenderingEnabled(Player player, bool enabled)
    {
        RenderingEnabled?.Invoke(player, enabled);
    }
}
