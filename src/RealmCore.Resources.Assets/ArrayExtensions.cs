namespace RealmCore.Resources.Assets;

public static class ArrayExtensions
{
    public static string CalculateChecksum(this byte[] data)
    {
        return string.Concat(MD5.HashData(data).Select(x => x.ToString("X2")));
    }
}
