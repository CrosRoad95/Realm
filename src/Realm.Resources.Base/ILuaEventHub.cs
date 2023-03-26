using SlipeServer.Server.Elements;
using System.Linq.Expressions;

namespace Realm.Resources.Base;

public interface ILuaEventHub<THub>
{
    void Invoke(Player player, Expression<Action<THub>> expression);
}
