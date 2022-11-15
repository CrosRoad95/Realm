namespace Realm.Scripting.Classes;

public class UlitityScriptingFunctions
{
    public UlitityScriptingFunctions()
    {

    }

    public async Task Delay(int miliseconds)
    {
        await Task.Delay(miliseconds);
    }
}
