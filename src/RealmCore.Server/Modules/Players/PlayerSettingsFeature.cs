namespace RealmCore.Server.Modules.Players;

public interface IPlayerSettingsFeature : IPlayerFeature, IEnumerable<UserSettingDto>
{
    event Action<IPlayerSettingsFeature, int, string>? Added;
    event Action<IPlayerSettingsFeature, int, string>? Changed;
    event Action<IPlayerSettingsFeature, int, string>? Removed;

    /// <summary>
    /// Return an array of all settings ids
    /// </summary>
    int[] SettingsIds { get; }
    void Set(int settingId, string value);
    bool Remove(int settingId);
    bool TryGet(int settingId, out string? value);
    string Get(int settingId);
    bool Has(int settingId);
    /// <summary>
    /// Removes all settings
    /// </summary>
    void Reset();
}

internal sealed class PlayerSettingsFeature : IPlayerSettingsFeature
{
    private readonly object _lock = new();
    private ICollection<UserSettingData> _settings = [];
    private readonly IPlayerUserFeature _playerUserFeature;

    public event Action<IPlayerSettingsFeature, int, string>? Added;
    public event Action<IPlayerSettingsFeature, int, string>? Changed;
    public event Action<IPlayerSettingsFeature, int, string>? Removed;

    public int[] SettingsIds
    {
        get
        {
            lock (_lock)
                return _settings.Select(x => x.SettingId).ToArray();
        }
    }

    public RealmPlayer Player { get; init; }
    public PlayerSettingsFeature(PlayerContext playerContext, IPlayerUserFeature playerUserFeature)
    {
        Player = playerContext.Player;
        playerUserFeature.SignedIn += HandleSignedIn;
        playerUserFeature.SignedOut += HandleSignedOut;
        _playerUserFeature = playerUserFeature;
    }

    private void HandleSignedIn(IPlayerUserFeature playerUserFeature, RealmPlayer _)
    {
        lock (_lock)
            _settings = playerUserFeature.User.Settings;
    }

    public void Reset()
    {
        foreach (var settingId in SettingsIds)
        {
            Remove(settingId);
        }
    }

    private void HandleSignedOut(IPlayerUserFeature playerUserFeature, RealmPlayer _)
    {
        Reset();
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
            {
                setting.Value = value;
                Changed?.Invoke(this, settingId, value);
            }
            else
            {
                _settings.Add(new UserSettingData
                {
                    SettingId = settingId,
                    Value = value
                });
                Added?.Invoke(this, settingId, value);
            }
        }
        _playerUserFeature.IncreaseVersion();
    }

    public string Get(int settingId)
    {
        lock (_lock)
            return _settings.First(x => x.SettingId == settingId).Value;
    }

    public bool TryGet(int settingId, out string? value)
    {
        lock (_lock)
        {
            var setting = _settings.FirstOrDefault(x => x.SettingId == settingId);
            if (setting != null)
            {
                value = setting.Value;
                return true;
            }
            value = null;
            return false;
        }
    }

    public bool Remove(int settingId)
    {
        lock (_lock)
        {
            var setting = _settings.First(x => x.SettingId == settingId);
            if (setting != null)
            {
                _settings.Remove(setting);
                Removed?.Invoke(this, setting.SettingId, setting.Value);
                _playerUserFeature.IncreaseVersion();
                return true;
            }
            return false;
        }
    }

    public IEnumerator<UserSettingDto> GetEnumerator()
    {
        lock (_lock)
            return new List<UserSettingDto>(_settings.Select(UserSettingDto.Map)).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
