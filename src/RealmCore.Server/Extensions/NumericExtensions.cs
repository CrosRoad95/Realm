namespace RealmCore.Server.Extensions;

public static class NumericExtensions
{
    public static string FormatMoney(this decimal amount, CultureInfo currencyCulture)
    {
        return amount.ToString("C", currencyCulture);
    }
}
