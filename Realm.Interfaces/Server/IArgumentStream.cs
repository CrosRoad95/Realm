namespace Realm.Interfaces.Server;

public interface ILuaEventContext
{
    IRPGPlayer Player { get; }

    object? GetValue<T>(int argumentIndex);
    void Response(params object[] obj);
}
