using SlipeServer.Packets.Definitions.Lua;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Events;

namespace RealmCore.Resources.AgnosticGuiSystem;


public delegate Task GuiChangedDelegate();

public interface IAgnosticGuiSystemService
{
    GuiChangedDelegate? GuiFilesChanged { get; set; }
    event Action<LuaEvent>? FormSubmitted;
    event Action<LuaEvent>? ActionExecuted;

    void CloseAllGuis(Player player);
    bool CloseGui(Player player, string gui, bool cursorless);
    bool OpenGui(Player player, string gui, bool cursorless, LuaValue? arg1 = null);
    void SendFormResponse(Player player, string id, string name, params object[] values);
    void SendStateChanged(Player player, string guiName, Dictionary<LuaValue, LuaValue> changes);
    void SetDebugToolsEnabled(Player player, bool enabled);
    Task UpdateGuiFiles();
    void HandleInternalSubmitForm(LuaEvent luaEvent);
    void HandleInternalActionExecuted(LuaEvent luaEvent);
}
