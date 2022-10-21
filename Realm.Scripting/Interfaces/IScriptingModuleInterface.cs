namespace Realm.Scripting.Interfaces;

public interface IScriptingModuleInterface
{
    void AddHostType(Type type, bool exposeGlobalMembers = false);
    void AddHostObject(string name, object @object, bool exposeGlobalMembers = false);
    void Execute(string code, string name);
    Task<object> ExecuteAsync(string code, string name);
    string GetTypescriptDefinition();
}
