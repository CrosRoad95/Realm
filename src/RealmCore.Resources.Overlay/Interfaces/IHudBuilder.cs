using RealmCore.Resources.Assets.Interfaces;
using RealmCore.Resources.Overlay.Builders.Interfaces;
using RealmCore.Resources.Overlay.Enums;
using System.Drawing;
using System.Linq.Expressions;
using System.Numerics;
using System.Reflection;

namespace RealmCore.Resources.Overlay.Interfaces;

public interface IHudBuilder<TState>
{
    float Right { get; }
    float Bottom { get; }

    Action<DynamicHudComponent>? DynamicHudComponentAdded { get; set; }

    IHudBuilder<TState> AddRectangle(Vector2 position, Size size, Color color);
    IHudBuilder<TState> AddText(string text, Vector2 position, Size size, Color? color = null, Size? scale = null, string font = "default", HorizontalAlign alignX = HorizontalAlign.Left, VerticalAlign alignY = VerticalAlign.Top);
    IHudBuilder<TState> AddText(string text, Vector2 position, Size size, Color? color = null, Size? scale = null, IFont? font = null, HorizontalAlign alignX = HorizontalAlign.Left, VerticalAlign alignY = VerticalAlign.Top);
    IHudBuilder<TState> AddText(Expression<Func<TState, string>> text, Vector2 position, Size size, Color? color = null, Size? scale = null, string font = "default", HorizontalAlign alignX = HorizontalAlign.Left, VerticalAlign alignY = VerticalAlign.Top);
    IHudBuilder<TState> AddText(Action<ITextHudBuilder<TState>> textBuilderCallback);
}

public interface IHudBuilder : IHudBuilder<object> { }