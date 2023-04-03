using Realm.Console.RazorGuis;
using Realm.Extensions.RazorGui;

namespace Realm.Console.Logic;

internal class TestLogic
{
    public TestLogic(IRazorGui razorGui)
    {
        var code = razorGui.RenderGui("login", "~/Login.cshtml", new RazorGuis.SampleState
        {

        });
        code.Wait();
        var res = code.Result;
        ;
    }
}
