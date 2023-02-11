using System.Drawing;
using System.Numerics;

namespace Realm.Resources.Overlay.Interfaces;

public interface IHudBuilder<TState>
{
    float Right { get; }
    float Bottom { get; }

    IHudBuilder<TState> AddRectangle(Vector2 position, Size size, Color color);
    IHudBuilder<TState> AddText(string text, Vector2 position, Size size, Color? color = null, Size? scale = null, string font = "default", string alignX = "left", string alignY = "top");
}
