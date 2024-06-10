namespace RealmCore.Resources.Assets;

public static class StreamExtensions
{
    public static byte[] ToArray(this Stream input)
    {
        using MemoryStream ms = new MemoryStream();
        input.CopyTo(ms);
        return ms.ToArray();
    }

    public static string CalculateChecksum(this Stream stream)
    {
        using var md5 = MD5.Create();
        var hashBytes = md5.ComputeHash(stream);
        stream.Position = 0;
        return Convert.ToHexString(hashBytes);
    }
}
