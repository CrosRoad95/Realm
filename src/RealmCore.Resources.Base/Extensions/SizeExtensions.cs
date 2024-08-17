namespace RealmCore.Resources.Base.Extensions;

public static class SizeExtensions
{
    public static LuaValue ToLuaArray(this Size size)
    {
        return new LuaValue[] { size.Width, size.Height };
    }
}
