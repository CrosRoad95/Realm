namespace Realm.Console.Data;

class UseItemData : ILuaValue
{
    public uint ItemId { get; set; } = default!;
    public void Parse(LuaValue luaValue)
    {
        ItemId = uint.Parse(luaValue.TableValue!["id"].IntegerValue!.ToString()!);
    }
}