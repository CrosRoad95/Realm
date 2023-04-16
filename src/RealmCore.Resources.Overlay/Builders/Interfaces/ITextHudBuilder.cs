using RealmCore.Resources.Assets.Interfaces;
using RealmCore.Resources.Overlay.Enums;
using SlipeServer.Packets.Definitions.Lua;
using System.Drawing;
using System.Numerics;
using System.Reflection;

namespace RealmCore.Resources.Overlay.Builders.Interfaces;

public interface ITextHudBuilder<TState>
{
    event Action<int, PropertyInfo>? DynamicHudComponentAdded;

    ITextHudBuilder<TState> WithColor(Color color);
    ITextHudBuilder<TState> WithFont(IFont font);
    ITextHudBuilder<TState> WithFont(string stringFont);
    ITextHudBuilder<TState> WithHorizontalAlign(HorizontalAlign alignX);
    ITextHudBuilder<TState> WithPosition(Vector2 position);
    ITextHudBuilder<TState> WithScale(Size? scale);
    ITextHudBuilder<TState> WithSize(Size size);
    ITextHudBuilder<TState> WithText(string text);
    ITextHudBuilder<TState> WithVerticalAlign(VerticalAlign alignY);
    internal LuaValue[] Build();
}
