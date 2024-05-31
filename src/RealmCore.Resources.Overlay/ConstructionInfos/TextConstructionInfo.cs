using RealmCore.Resources.Overlay.Extensions;
using System.Reflection;

namespace RealmCore.Resources.Overlay.ConstructionInfos;

internal struct TextConstructionInfo
{
    public bool isDynamic;
    public List<PropertyInfo> propertyInfos;
    public int? parentId;
    public int id;
    public bool isComputed;
    public string text;
    public Vector2 position;
    public Size size;
    public Color color;
    public Size scale;
    public LuaValue font;
    public HorizontalAlign alignX;
    public VerticalAlign alignY;

    public LuaValue[] AsLuaValue()
    {
        return
            [
                isComputed ? "computedValue" : "text",
                id,
                text,
                position.X,
                position.Y,
                size.Width,
                size.Height,
                color.ToLuaColor(),
                scale.Width,
                scale.Height,
                font,
                alignX.AsString(),
                alignY.AsString()
            ];
    }
}