using SlipeServer.Server.Elements;
using System.Drawing;

namespace Realm.Resources.ElementOutline;

public class ElementOutlineService
{
    internal event Action<Player, Element, Color>? OutlineChanged;
    internal event Action<Player, Element>? OutlineRemoved;

    public void SetElementOutlineForPlayer(Player player, Element target, Color color)
    {
        OutlineChanged?.Invoke(player, target, color);
    }

    public void RemoveElementOutlineForPlayer(Player player, Element target)
    {
        OutlineRemoved?.Invoke(player, target);
    }
}
