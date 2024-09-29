using System.Numerics;

namespace RealmCore.Resources.Base;

public class PositionContext
{
    private readonly LuaValue _luaValue;

    internal static string LuaControllerCode = """
        function getPositionFromContext(context)
            if(context.type == "constantPosition")then
                return unpack(context.position)
            end
        end
        """;

    public PositionContext(Vector3 position)
    {
        _luaValue = new Dictionary<LuaValue, LuaValue>
        {
            ["type"] = "constantPosition",
            ["position"] = LuaValue.ArrayFromVector(position)
        };
    }

    public PositionContext(Element element, Vector3 offset)
    {
        _luaValue = new Dictionary<LuaValue, LuaValue>
        {
            ["type"] = "elementWithOffset",
            ["element"] = element.Id,
            ["offset"] = LuaValue.ArrayFromVector(offset)
        };
    }

    public static implicit operator PositionContext(Vector3 position)
    {
        return new PositionContext(position);
    }

    public LuaValue AsLuaValue() => _luaValue;
}