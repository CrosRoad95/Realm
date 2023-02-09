namespace Realm.Server.Seeding;

public interface ISeederProviderBase
{
    string SeedKey { get; }
}

public interface ISeederProvider : ISeederProviderBase
{
    void Seed(string key, string id, JObject data);
}

public interface IAsyncSeederProvider : ISeederProviderBase
{
    Task SeedAsync(string key, string id, JObject data);
}
