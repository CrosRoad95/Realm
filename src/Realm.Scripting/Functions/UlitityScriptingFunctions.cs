namespace Realm.Module.Scripting.Functions;

[NoDefaultScriptAccess]
public class UlitityScriptingFunctions
{
    public UlitityScriptingFunctions()
    {

    }

    [ScriptMember("delay")]
    public async Task Delay(int miliseconds)
    {
        await Task.Delay(miliseconds);
    }
}
