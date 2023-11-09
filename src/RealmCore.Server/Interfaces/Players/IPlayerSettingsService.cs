
namespace RealmCore.Server.Interfaces.Players;

public interface IPlayerSettingsService : IPlayerService, IEnumerable<UserSettingDTO>
{
    event Action<IPlayerSettingsService, int, string>? Changed;
    event Action<IPlayerSettingsService, int, string>? Removed;

    void Set(int settingId, string value);
    bool Remove(int settingId);
    bool TryGet(int settingId, out string? value);
    string Get(int settingId);
    bool Has(int settingId);
}
