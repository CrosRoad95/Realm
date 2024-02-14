using System.Security.Cryptography;

namespace RealmCore.Resources.Assets;

internal static class Utilities
{
    public static string CreateMD5(Stream stream)
    {
        using var md5 = MD5.Create();
        var hashBytes = md5.ComputeHash(stream);
        stream.Position = 0;
        return Convert.ToHexString(hashBytes);
    }

    public static string CreateMD5(string input)
    {
        byte[] inputBytes = System.Text.Encoding.UTF8.GetBytes(input);
        byte[] hashBytes = MD5.HashData(inputBytes);

        return Convert.ToHexString(hashBytes);
    }

    public static byte[] ReadFully(Stream input)
    {
        using MemoryStream ms = new();
        input.CopyTo(ms);
        ms.Position = 0;
        return ms.ToArray();
    }
}