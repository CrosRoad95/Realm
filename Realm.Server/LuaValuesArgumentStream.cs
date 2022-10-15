namespace Realm.Server;

public class LuaEventContextFactory
{
    private readonly Dictionary<Type, Func<LuaValue, object>> _implicitlyCastableTypes = new();
    private readonly IElementCollection _elementCollection;
    public LuaEventContextFactory(IElementCollection elementCollection)
    {
        _elementCollection = elementCollection;
        IndexImplicitlyCastableTypes();
    }

    public ILuaEventContext CreateContextFromLuaEvent(LuaEvent luaEvent) => new LuaEventContext(luaEvent, ConvertLuaValue);

    private void IndexImplicitlyCastableTypes()
    {
        foreach (var method in typeof(LuaValue).GetMethods().Where(x => x.Name == "op_Explicit"))
            _implicitlyCastableTypes[method.ReturnType] = (value) => method.Invoke(null, new object[] { value })!;
    }

    private object? ConvertLuaValue(Type type, LuaValue value)
    {
        if (type.IsAssignableTo(typeof(ILuaValue)))
        {
            var instance = (ILuaValue)Activator.CreateInstance(type)!;
            instance.Parse(value);
            return instance;
        }
        else if (type.IsAssignableTo(typeof(Element)) && value.ElementId.HasValue)
            return _elementCollection.Get(value.ElementId!.Value);
        else if (_implicitlyCastableTypes.ContainsKey(type))
            return _implicitlyCastableTypes[type](value);
        else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>) && value.TableValue != null)
            return value.TableValue.ToDictionary(
                x => ConvertLuaValue(type.GenericTypeArguments.First(), x.Key) ?? new object(),
                x => ConvertLuaValue(type.GenericTypeArguments.Last(), x.Value));
        else if (type.IsAssignableTo(typeof(IEnumerable<>)) && value.TableValue != null)
            return value.TableValue.Values.Select(x => ConvertLuaValue(type.GenericTypeArguments.First(), value));
        else if (type.IsEnum && value.IntegerValue.HasValue)
            return Enum.ToObject(type, Convert.ChangeType(value.IntegerValue.Value, Enum.GetUnderlyingType(type) ?? typeof(int)));
        else
            return null;
    }
}

public class LuaEventContext : ILuaEventContext
{
    private readonly LuaEvent _luaEvent;
    private readonly Func<Type, LuaValue, object?> _converter;
    private readonly string _id;

    public IRPGPlayer Player => (RPGPlayer)_luaEvent.Player;
    public LuaEventContext(LuaEvent luaEvent, Func<Type, LuaValue, object?> converter)
    {
        _luaEvent = luaEvent;
        _converter = converter;
        _id = (GetValue<string>(0) as string) ?? throw new Exception("id not found.");
    }

    public object? GetValue<T>(int argumentIndex)
    {
        return _converter(typeof(T), _luaEvent.Parameters[argumentIndex]);
    }

    public void Response(params object[] values)
    {
        Player.TriggerClientEvent($"{_luaEvent.Name}Response", values.Prepend(_id).ToArray());
    }
}
