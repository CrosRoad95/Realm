using Realm.Resources.Assets.Interfaces;
using Realm.Resources.Overlay.Enums;
using System.Drawing;
using System.Linq.Expressions;
using System.Numerics;
using System.Reflection;

namespace Realm.Resources.Overlay.Interfaces;

public interface IHudBuilder<TState>
{
    float Right { get; }
    float Bottom { get; }

    event Action<int, PropertyInfo>? DynamicHudComponentAdded;

    IHudBuilder<TState> AddRectangle(Vector2 position, Size size, Color color);
    IHudBuilder<TState> AddText(string text, Vector2 position, Size size, Color? color = null, Size? scale = null, string font = "default", HorizontalAlign alignX = HorizontalAlign.Left, VerticalAlign alignY = VerticalAlign.Top);
    IHudBuilder<TState> AddText(string text, Vector2 position, Size size, Color? color = null, Size? scale = null, IFont? font = null, HorizontalAlign alignX = HorizontalAlign.Left, VerticalAlign alignY = VerticalAlign.Top);
    IHudBuilder<TState> AddText(Expression<Func<TState, string>> text, Vector2 position, Size size, Color? color = null, Size? scale = null, string font = "default", HorizontalAlign alignX = HorizontalAlign.Left, VerticalAlign alignY = VerticalAlign.Top);
}

public interface IHudBuilder : IHudBuilder<object> { }