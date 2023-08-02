using RealmCore.Resources.Assets.Interfaces;
using RealmCore.Resources.Overlay.Enums;
using SlipeServer.Packets.Definitions.Lua;
using System.Drawing;
using System.Linq.Expressions;
using System.Numerics;

namespace RealmCore.Resources.Overlay.Builders.Interfaces;

public interface ITextHudBuilder<TState>
{
    ITextHudBuilder<TState> WithColor(Color color);
    ITextHudBuilder<TState> WithFont(IFont font);
    ITextHudBuilder<TState> WithFont(string stringFont);
    ITextHudBuilder<TState> WithHorizontalAlign(HorizontalAlign alignX);
    ITextHudBuilder<TState> WithPosition(Vector2 position);
    ITextHudBuilder<TState> WithScale(Size? scale);
    ITextHudBuilder<TState> WithSize(Size size);
    ITextHudBuilder<TState> WithText(string text);
    ITextHudBuilder<TState> WithText(Expression<Func<TState, string>> expression);
    ITextHudBuilder<TState> WithVerticalAlign(VerticalAlign alignY);
    internal Action<DynamicHudComponent>? DynamicHudComponentAdded { get; set; }
    internal LuaValue[] Build();
    ITextHudBuilder<TState> WithComputedValue(ComputedValueType computedValueType);
}
