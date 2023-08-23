namespace RealmCore.Console.Data;

class RegisterData : ILuaValue
{
    public string Login { get; set; } = default!;
    public string Password { get; set; } = default!;
    public string RepeatPassword { get; set; } = default!;
    public void Parse(LuaValue luaValue)
    {
        Login = luaValue.TableValue!["login"].StringValue!;
        Password = luaValue.TableValue!["password"].StringValue!;
        RepeatPassword = luaValue.TableValue!["repeatPassword"].StringValue!;
    }
}