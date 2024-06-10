namespace RealmCore.Resources.Assets;

public static class StringExtensions
{
    public static string CalculateChecksum(this string input)
    {
        byte[] inputBytes = System.Text.Encoding.UTF8.GetBytes(input);
        byte[] hashBytes = MD5.HashData(inputBytes);

        return Convert.ToHexString(hashBytes);
    }
}
