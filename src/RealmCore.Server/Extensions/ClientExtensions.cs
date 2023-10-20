namespace RealmCore.Server.Extensions;

public static class ClientExtensions
{
    public static string GetSerial(this IClient client) => client.Serial ?? throw new SerialNullException();

    public static string TryGetSerial(this IClient client) => client.Serial ?? "<no serial>";
}
