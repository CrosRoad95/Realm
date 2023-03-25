using SlipeServer.Server.Elements;
using System.Drawing;

namespace Realm.Resources.ElementOutline;

internal sealed class ElementOutlineService : IElementOutlineService
{
    public event Action<Player, Element, Color>? OutlineChanged;
    public event Action<Player, Element>? OutlineRemoved;

    public void SetElementOutlineForPlayer(Player player, Element target, Color color)
    {
        OutlineChanged?.Invoke(player, target, color);
    }

    public void RemoveElementOutlineForPlayer(Player player, Element target)
    {
        OutlineRemoved?.Invoke(player, target);
    }
}
