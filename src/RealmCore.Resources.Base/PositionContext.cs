using System.Numerics;

namespace RealmCore.Resources.Base;

public class PositionContext
{
    private readonly LuaValue _luaValue;

    internal static string LuaControllerCode = """
        function getOffsetFromXYZ( mat, vec )
            -- make sure our matrix is setup correctly 'cos MTA used to set all of these to 1.
            mat[1][4] = 0
            mat[2][4] = 0
            mat[3][4] = 0
            mat[4][4] = 1
            local offX = vec[1] * mat[1][1] + vec[2] * mat[2][1] + vec[3] * mat[3][1] + mat[4][1]
            local offY = vec[1] * mat[1][2] + vec[2] * mat[2][2] + vec[3] * mat[3][2] + mat[4][2]
            local offZ = vec[1] * mat[1][3] + vec[2] * mat[2][3] + vec[3] * mat[3][3] + mat[4][3]
            return offX, offY, offZ
        end

        function getPositionFromContext(context)
            if(context.type == "constantPosition")then
                return unpack(context.position);
            elseif(context.type == "element")then
                return getElementPosition(context.element);
            elseif(context.type == "elementWithOffset")then
                return getOffsetFromXYZ(getElementMatrix(context.element), context.offset);
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

    public PositionContext(Element element, Vector3? offset = null)
    {
        if (offset != null)
        {
            _luaValue = new Dictionary<LuaValue, LuaValue>
            {
                ["type"] = "elementWithOffset",
                ["element"] = element.Id,
                ["offset"] = LuaValue.ArrayFromVector(offset.Value)
            };
        }
        else
        {
            _luaValue = new Dictionary<LuaValue, LuaValue>
            {
                ["type"] = "element",
                ["element"] = element.Id,
            };
        }
    }

    public static implicit operator PositionContext(Vector3 position)
    {
        return new PositionContext(position);
    }

    public LuaValue AsLuaValue() => _luaValue;
}