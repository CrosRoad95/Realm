namespace Realm.Interfaces.Scripting;

public interface IScripting
{
    void Execute(string code, string name);
    Task<object> ExecuteAsync(string code, string name);
    string GetTypescriptDefinition();
}
