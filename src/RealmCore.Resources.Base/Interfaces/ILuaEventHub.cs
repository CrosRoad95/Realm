using SlipeServer.Server.Elements;
using System.Linq.Expressions;

namespace RealmCore.Resources.Base.Interfaces;

public interface ILuaEventHub<THub>
{
    void Broadcast(Expression<Action<THub>> expression, Element? source = null);
    void Invoke(Player player, Expression<Action<THub>> expression, Element? source = null);
}
