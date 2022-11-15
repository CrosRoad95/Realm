namespace Realm.Scripting.Classes;

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
