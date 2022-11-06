using SlipeServer.Server.Events;

namespace Realm.Resources.AgnosticGuiSystem;

public class AgnosticGuiSystemService
{
    public event Action<LuaEvent>? FormSubmitted;
    public event Action<LuaEvent>? GuiCloseRequested;
    public event Action<LuaEvent>? GuiNavigationRequested;
    public AgnosticGuiSystemService()
    {

    }

    internal void HandleInternalSubmitForm(LuaEvent luaEvent)
    {
        FormSubmitted?.Invoke(luaEvent);
    }


    internal void HandleInternalRequestGuiClose(LuaEvent luaEvent)
    {
        GuiCloseRequested?.Invoke(luaEvent);
    }

    internal void HandleInternalNavigateToGui(LuaEvent luaEvent)
    {
        GuiNavigationRequested?.Invoke(luaEvent);
    }
}
