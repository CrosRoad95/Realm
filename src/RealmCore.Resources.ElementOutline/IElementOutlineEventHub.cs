using SlipeServer.Server.Elements;
using System.Drawing;

namespace RealmCore.Resources.ElementOutline;

internal interface IElementOutlineEventHub
{
    void SetOutlines(Element[] elements, Color[] color);
    void SetOutlineForElement(Element element, Color color);
    void SetOutline(Color color);
    void RemoveOutline();
    void SetRenderingEnabled(bool enabled);
}
