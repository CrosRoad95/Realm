namespace Realm.Module.Scripting.Interfaces;

public interface IScriptingModuleInterface
{
    void AddHostType(Type type, bool exposeGlobalMembers = false);
    void AddHostObject(string name, object @object, bool exposeGlobalMembers = false);
    Task<object?> ExecuteAsync(string code, string name);
    string GetTypescriptDefinition();
    Task Start();
}
