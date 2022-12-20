using SlipeServer.Packets.Definitions.Lua;
using SlipeServer.Server.Events;

namespace Realm.Domain.Components.Players.Gui;

public class LoginGuiComponent : GuiComponent
{
    private class LoginData
    {
        public LoginData(LuaValue luaValue)
        {

        }
    }

    public LoginGuiComponent() : base("login")
    {

    }

    protected override Task HandleForm(string formName, LuaValue data)
    {
        switch(formName)
        {
            case "login":
                var loginData = new LoginData(data);
            default:
                throw new NotImplementedException();
        }
    }
}
