namespace Realm.Scripting.Interfaces;

public interface IScripting
{
    void AddType(Type type);
    void Execute(string code);
    Task<object> ExecuteAsync(string code);
}
