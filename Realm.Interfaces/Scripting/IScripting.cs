namespace Realm.Interfaces.Scripting;

public interface IScripting
{
    void AddHostType(string name, Type type);
    void Execute(string code);
    Task<object> ExecuteAsync(string code);
}
