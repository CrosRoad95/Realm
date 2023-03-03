using Realm.Resources.Assets.Interfaces;
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
    IHudBuilder<TState> AddText(string text, Vector2 position, Size size, Color? color = null, Size? scale = null, string font = "default", string alignX = "left", string alignY = "top");
    IHudBuilder<TState> AddText(string text, Vector2 position, Size size, Color? color = null, Size? scale = null, IFont? font = null, string alignX = "left", string alignY = "top");
    IHudBuilder<TState> AddText(Expression<Func<TState, string>> text, Vector2 position, Size size, Color? color = null, Size? scale = null, string font = "default", string alignX = "left", string alignY = "top");
}

public interface IHudBuilder : IHudBuilder<object> { }