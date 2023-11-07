
namespace RealmCore.Server.Interfaces.Players;

public interface IPlayerSettingsService
{
    ICollection<int> Settings { get; }
    RealmPlayer Player { get; }

    event Action<IPlayerSettingsService, int, string>? Changed;
    event Action<IPlayerSettingsService, int, string>? Removed;

    void Set(int settingId, string value);
    bool Remove(int settingId);
    bool TryGet(int settingId, out string? value);
    string Get(int settingId);
    bool Has(int settingId);
}
