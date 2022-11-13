namespace Realm.Scripting.Classes;

public class UlitityFunctions
{
    public UlitityFunctions()
    {

    }

    public async Task Delay(int miliseconds)
    {
        await Task.Delay(miliseconds);
    }
}
