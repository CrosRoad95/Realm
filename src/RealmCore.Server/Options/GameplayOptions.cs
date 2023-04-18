namespace RealmCore.Server.Options;

public class GameplayOptions
{
    public long MoneyLimit { get; set; }
    public byte MoneyPrecision { get; set; }
    public CultureInfo CurrencyCulture { get; set; }
    public CultureInfo Culture { get; set; }
    public uint DefaultInventorySize { get; set; }
    public int BanType { get; set; }
    public string Watermark { get; set; }
}
