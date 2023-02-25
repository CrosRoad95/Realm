namespace Realm.Configuration;

public interface IRealmConfigurationProvider
{
    T? Get<T>(string name);
    T GetRequired<T>(string name);
    IConfigurationSection GetSection(string name);
}
