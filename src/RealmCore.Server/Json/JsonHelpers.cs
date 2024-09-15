namespace RealmCore.Server.Json;

public static class JsonHelpers
{
    public static string? Serialize(object? obj) => obj != null ? JsonConvert.SerializeObject(obj, DefaultDeserializationSettings) : null;
    public static T? Deserialize<T>(string? data) where T : class => data != null ? JsonConvert.DeserializeObject<T>(data, DefaultDeserializationSettings) : default;

    public static JsonSerializerSettings DefaultDeserializationSettings = new JsonSerializerSettings
    {
        TypeNameHandling = TypeNameHandling.All,
    };
}
