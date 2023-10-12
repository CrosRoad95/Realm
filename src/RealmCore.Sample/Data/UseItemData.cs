using RealmCore.Server.Enums;

namespace RealmCore.Sample.Data;

class UseItemData : ILuaValue
{
    public uint ItemId { get; set; } = default!;
    public string LocalId { get; set; } = default!;
    public ItemAction ItemAction { get; set; } = default!;

    public void Parse(LuaValue luaValue)
    {
        ItemId = uint.Parse(luaValue.TableValue!["id"].IntegerValue!.ToString()!);
        LocalId = luaValue.TableValue!["localId"].StringValue!.ToString()!;
        ItemAction = Enum.Parse<ItemAction>(luaValue.TableValue!["action"].IntegerValue!.ToString()!);
    }
}