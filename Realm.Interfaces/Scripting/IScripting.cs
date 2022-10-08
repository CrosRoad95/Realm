namespace Realm.Interfaces.Scripting;

public interface IScripting
{
    void Execute(string code);
    Task<object> ExecuteAsync(string code);
    string GetTypescriptDefinition();
}
