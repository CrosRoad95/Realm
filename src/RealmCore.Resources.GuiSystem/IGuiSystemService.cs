using SlipeServer.Packets.Definitions.Lua;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Events;

namespace RealmCore.Resources.GuiSystem;

public delegate Task GuiChangedDelegate();

public interface IGuiSystemService
{
    GuiChangedDelegate? GuiFilesChanged { get; set; }
    event Action<LuaEvent>? FormSubmitted;
    event Action<LuaEvent>? ActionExecuted;

    void CloseAllGui(Player player);
    bool CloseGui(Player player, string gui, bool cursorLess);
    bool OpenGui(Player player, string gui, bool cursorLess, LuaValue? arg1 = null);
    void SendFormResponse(Player player, string id, string name, params object[] values);
    void SendStateChanged(Player player, string guiName, Dictionary<LuaValue, LuaValue> changes);
    void SetDebugToolsEnabled(Player player, bool enabled);
    Task UpdateGuiFiles();
    void HandleInternalSubmitForm(LuaEvent luaEvent);
    void HandleInternalActionExecuted(LuaEvent luaEvent);
}
