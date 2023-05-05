namespace RealmCore.Server.Extensions;

public static class DecimalExtensions
{
    public static decimal Truncate(this decimal d, byte decimals)
    {
        decimal r = Math.Round(d, decimals);

        if (d > 0 && r > d)
        {
            return r - new decimal(1, 0, 0, false, decimals);
        }
        else if (d < 0 && r < d)
        {
            return r + new decimal(1, 0, 0, false, decimals);
        }

        return r;
    }
}
