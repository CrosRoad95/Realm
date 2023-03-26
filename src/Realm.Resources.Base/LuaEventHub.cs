using SlipeServer.Packets.Definitions.Lua;
using SlipeServer.Server;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Mappers;
using SlipeServer.Server.Resources;
using SlipeServer.Server.Services;
using System.Linq.Expressions;

namespace Realm.Resources.Base;

public class LuaEventHub<THub, TResource> : ILuaEventHub<THub> where TResource: Resource
{
    private readonly TResource _resource;
    private readonly LuaEventService _luaEventService;
    private readonly LuaValueMapper _luaValueMapper;

    public LuaEventHub(MtaServer server, LuaEventService luaEventService, LuaValueMapper luaValueMapper)
    {
        _resource = server.GetAdditionalResource<TResource>();
        _luaEventService = luaEventService;
        _luaValueMapper = luaValueMapper;
    }

    public void Invoke(Player player, Expression<Action<THub>> expression)
    {
        var (eventName, values) = ConvertExpression(expression);
        _luaEventService.TriggerEventFor(player, eventName, player, values.ToArray());
    }

    private (string, IEnumerable<LuaValue>) ConvertExpression(Expression<Action<THub>> expression)
    {
        if (expression is not LambdaExpression lambdaExpression)
            throw new NotSupportedException();

        if (lambdaExpression.Body is not MethodCallExpression methodCallExpression)
            throw new NotSupportedException();

        var luaValues = methodCallExpression.Arguments.Select(ConvertExpressionToLuaValue);
        var methodName = methodCallExpression.Method.Name;
        var eventName = $"internalHub{_resource.Name}{methodName}";
        return (eventName, luaValues);
    }

    private LuaValue ConvertExpressionToLuaValue(Expression expression)
    {
        switch(expression)
        {
            case ConstantExpression constantExpression:
                return _luaValueMapper.Map(constantExpression.Value);
            default:
                throw new NotSupportedException();
        }
    }
}
