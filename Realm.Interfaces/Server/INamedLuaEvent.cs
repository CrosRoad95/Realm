namespace Realm.Interfaces.Server;

public interface INamedLuaEvent
{
    abstract static string Name { get; }
}
