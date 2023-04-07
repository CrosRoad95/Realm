using SlipeServer.Packets.Definitions.Lua;
using SlipeServer.Server;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Mappers;
using SlipeServer.Server.Resources;
using SlipeServer.Server.Services;
using System.Linq.Expressions;
using System.Reflection;

namespace RealmCore.Resources.Base;

public class LuaEventHub<THub, TResource> : ILuaEventHub<THub> where TResource : Resource
{
    private readonly TResource _resource;
    private readonly LuaEventService _luaEventService;
    private readonly LuaValueMapper _luaValueMapper;
    private readonly RootElement _rootElement;

    public LuaEventHub(MtaServer server, LuaEventService luaEventService, LuaValueMapper luaValueMapper, RootElement rootElement)
    {
        _resource = server.GetAdditionalResource<TResource>();
        _luaEventService = luaEventService;
        _luaValueMapper = luaValueMapper;
        _rootElement = rootElement;
    }

    public void Invoke(Player player, Expression<Action<THub>> expression, Element? source = null)
    {
        var (eventName, values) = ConvertExpression(expression);
        _luaEventService.TriggerEventFor(player, eventName, source ?? player, values.ToArray());
    }

    public void Broadcast(Expression<Action<THub>> expression, Element? source = null)
    {
        var (eventName, values) = ConvertExpression(expression, "internalHubBroadcast");
        _luaEventService.TriggerEvent(eventName, source ?? _rootElement, values.ToArray());
    }

    private (string, IEnumerable<LuaValue>) ConvertExpression(Expression<Action<THub>> expression, string baseName = "internalHub")
    {
        if (expression is not LambdaExpression lambdaExpression)
            throw new NotSupportedException();

        if (lambdaExpression.Body is not MethodCallExpression methodCallExpression)
            throw new NotSupportedException();

        var luaValues = methodCallExpression.Arguments.Select(ConvertExpressionToLuaValue);
        var methodName = methodCallExpression.Method.Name;
        var eventName = $"{baseName}{_resource.Name}{methodName}";
        return (eventName, luaValues);
    }

    private LuaValue ConvertExpressionToLuaValue(Expression expression)
    {
        switch (expression)
        {
            case ConstantExpression constantExpression:
                return _luaValueMapper.Map(constantExpression.Value);
            case MemberExpression memberExpression:
                if (memberExpression.Member is FieldInfo fieldInfo)
                {
                    var value = fieldInfo.GetValue(((ConstantExpression)memberExpression.Expression).Value);
                    return _luaValueMapper.Map(value);
                }
                return ConvertExpressionToLuaValue(memberExpression.Expression);
            default:
                throw new NotSupportedException();
        }
    }
}
