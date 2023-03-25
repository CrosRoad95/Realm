using SlipeServer.Server.Elements;
using System.Drawing;

namespace Realm.Resources.ElementOutline;

public interface IElementOutlineService
{
    internal event Action<Player, Element, Color>? OutlineChanged;
    internal event Action<Player, Element>? OutlineRemoved;

    void RemoveElementOutlineForPlayer(Player player, Element target);
    void SetElementOutlineForPlayer(Player player, Element target, Color color);
}
