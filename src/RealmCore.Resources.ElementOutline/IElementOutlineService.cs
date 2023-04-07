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
