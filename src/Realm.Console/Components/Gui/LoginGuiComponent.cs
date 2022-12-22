namespace Realm.Console.Components.Gui;

public sealed class LoginGuiComponent : GuiComponent
{
    private class LoginData : ILuaValue
    {
        public void Parse(LuaValue luaValue)
        {
            throw new NotImplementedException();
        }
    }

    public LoginGuiComponent() : base("login")
    {

    }

    protected override async Task HandleForm(IFormContext formContext)
    {
        switch (formContext.FormName)
        {
            case "login":
                var loginData = formContext.GetData<LoginData>();
                break;
            default:
                throw new NotImplementedException();
        }
    }
}
