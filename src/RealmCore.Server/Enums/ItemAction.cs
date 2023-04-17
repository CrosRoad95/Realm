namespace RealmCore.Server.Enums;

[Flags]
public enum ItemAction
{
    None = 0,
    Use = 1 << 0,       // 1
    Drop = 1 << 1,      // 2
    Eat = 1 << 2,       // 4
    Drink = 1 << 3,     // 8
    Place = 1 << 4,     // 16
    Equip = 1 << 5,     // 32
    Unequip = 1 << 6,   // 64
    Examine = 1 << 7,   // 128
    Break = 1 << 8,     // 256
    Repair = 1 << 9,    // 512
    Open = 1 << 10,     // 1024
    Close = 1 << 11,    // 2048
    Select = 1 << 12,   // 4096
}
