using System.Collections;

namespace RealmCore.Server.Services.Players;

internal class PlayerSettingsService : IPlayerSettingsService, IDisposable
{
    private object _lock = new();
    private ICollection<UserSettingData> _settings = [];
    private readonly IPlayerUserService _playerUserService;

    public RealmPlayer Player { get; }
    public event Action<IPlayerSettingsService, int, string>? Changed;
    public event Action<IPlayerSettingsService, int, string>? Removed;

    public PlayerSettingsService(PlayerContext playerContext, IPlayerUserService playerUserService)
    {
        Player = playerContext.Player;
        playerUserService.SignedIn += HandleSignedIn;
        playerUserService.SignedOut += HandleSignedOut;
        _playerUserService = playerUserService;
    }

    private void HandleSignedIn(IPlayerUserService playerUserService, RealmPlayer _)
    {
        lock (_lock)
            _settings = playerUserService.User.Settings;
    }

    private void HandleSignedOut(IPlayerUserService playerUserService, RealmPlayer _)
    {
        lock (_lock)
            _settings = [];
    }

    public bool Has(int settingId)
    {
        lock (_lock)
            return _settings.Any(x => x.SettingId == settingId);
    }

    public void Set(int settingId, string value)
    {
        lock (_lock)
        {
            var setting = _settings.FirstOrDefault(x => x.SettingId == settingId);
            if (setting != null)
                setting.Value = value;
            else
                _settings.Add(new UserSettingData
                {
                    SettingId = settingId,
                    Value = value
                });
        }
        _playerUserService.IncreaseVersion();
        Changed?.Invoke(this, settingId, value);
    }

    public string Get(int settingId)
    {
        lock(_lock)
            return _settings.First(x => x.SettingId == settingId).Value;
    }

    public bool TryGet(int settingId, out string value)
    {
        var setting = _settings.FirstOrDefault(x => x.SettingId == settingId);
        if(setting != null)
        {
            value = setting.Value;
            return true;
        }
        value = string.Empty;
        return false;
    }

    public bool Remove(int settingId)
    {
        var setting = _settings.First(x => x.SettingId == settingId);
        if (setting != null)
        {
            _settings.Remove(setting);
            _playerUserService.IncreaseVersion();
            return true;
        }
        return false;
    }

    public void Dispose()
    {

    }

    public IEnumerator<UserSettingDTO> GetEnumerator()
    {
        lock (_lock)
            return new List<UserSettingDTO>(_settings.Select(UserSettingDTO.Map)).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
