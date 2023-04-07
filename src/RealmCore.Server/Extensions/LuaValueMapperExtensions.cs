namespace RealmCore.Server.Extensions;

internal static class LuaValueMapperExtensions
{
    public static LuaValue UniversalMap(this LuaValueMapper mapper, object? obj)
    {
        if (obj == null)
            return new LuaValue
            {
                LuaType = LuaType.Nil
            };

        var result = new Dictionary<LuaValue, LuaValue>();
        foreach (var property in obj.GetType().GetProperties())
        {
            result[property.Name] = mapper.Map(property.GetValue(obj));
        }
        return new LuaValue(result);
    }
}
