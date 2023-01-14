using static Realm.Domain.Registries.ItemRegistryEntry;

namespace Realm.Console.Data;

class UseItemData : ILuaValue
{
    public uint ItemId { get; set; } = default!;
    public ItemAction ItemAction { get; set; } = default!;

    public void Parse(LuaValue luaValue)
    {
        ItemId = uint.Parse(luaValue.TableValue!["id"].IntegerValue!.ToString()!);
        ItemAction = Enum.Parse<ItemAction>(luaValue.TableValue!["action"].IntegerValue!.ToString()!);
    }
}