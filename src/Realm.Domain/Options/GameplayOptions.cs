namespace Realm.Domain.Options;

public class GameplayOptions
{
    public long MoneyLimit { get; set; }
    public byte MoneyPrecision { get; set; }
    public string CurrencyCulture { get; set; }
    public uint DefaultInventorySize { get; set; }
}
