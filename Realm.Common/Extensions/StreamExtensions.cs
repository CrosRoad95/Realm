namespace Realm.Common.Extensions;

public static class StreamExtensions
{
    public static byte[] ToByteArray(this Stream input)
    {
        using var ms = new MemoryStream();
        input.CopyTo(ms);
        return ms.ToArray();
    }
}
