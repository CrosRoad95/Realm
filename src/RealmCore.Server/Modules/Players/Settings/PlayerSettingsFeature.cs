namespace RealmCore.Server.Modules.Players.Settings;

public interface IPlayerSettingsFeature : IPlayerFeature, IEnumerable<UserSettingDto>
{
    event Action<IPlayerSettingsFeature, UserSettingDto>? Added;
    event Action<IPlayerSettingsFeature, UserSettingDto>? Changed;
    event Action<IPlayerSettingsFeature, UserSettingDto>? Removed;

    /// <summary>
    /// Return an array of all settings ids
    /// </summary>
    int[] SettingsIds { get; }
    void Set(int settingId, string value);
    bool TryGet(int settingId, out string? value);
    bool Has(int settingId);
    /// <summary>
    /// Removes all settings
    /// </summary>
    void Reset();
    bool TryRemove(int settingId);
}

internal sealed class PlayerSettingsFeature : IPlayerSettingsFeature, IUsesUserPersistentData
{
    private readonly object _lock = new();
    private ICollection<UserSettingData> _settings = [];

    public event Action<IPlayerSettingsFeature, UserSettingDto>? Added;
    public event Action<IPlayerSettingsFeature, UserSettingDto>? Changed;
    public event Action<IPlayerSettingsFeature, UserSettingDto>? Removed;
    public event Action? VersionIncreased;

    public int[] SettingsIds
    {
        get
        {
            lock (_lock)
                return _settings.Select(x => x.SettingId).ToArray();
        }
    }

    public RealmPlayer Player { get; init; }
    public PlayerSettingsFeature(PlayerContext playerContext)
    {
        Player = playerContext.Player;
    }


    public void LogIn(UserData userData)
    {
        foreach (var item in userData.Settings)
        {
            Set(item.SettingId, item.Value);
        }
        lock (_lock)
            _settings = userData.Settings;
    }

    public void Reset()
    {
        foreach (var settingId in SettingsIds)
        {
            TryRemove(settingId);
        }
    }

    public bool Has(int settingId)
    {
        lock (_lock)
            return _settings.Any(x => x.SettingId == settingId);
    }

    public void Set(int settingId, string value)
    {
        bool added = false;
        UserSettingData? setting;
        lock (_lock)
        {
            setting = _settings.FirstOrDefault(x => x.SettingId == settingId);
            if (setting != null)
            {
                setting.Value = value;
            }
            else
            {
                setting = new UserSettingData
                {
                    SettingId = settingId,
                    Value = value
                };

                _settings.Add(setting);
            }
        }

        if (added)
        {
            Added?.Invoke(this, UserSettingDto.Map(setting));
        }
        else
        {
            Changed?.Invoke(this, UserSettingDto.Map(setting));
        }
        VersionIncreased?.Invoke();
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
        }
        value = null;
        return false;
    }

    public bool TryRemove(int settingId)
    {
        UserSettingData? setting;
        lock (_lock)
        {
            setting = _settings.FirstOrDefault(x => x.SettingId == settingId);
            if (setting != null)
            {
                _settings.Remove(setting);
            }
        }

        if (setting != null)
        {
            Removed?.Invoke(this, UserSettingDto.Map(setting));
            VersionIncreased?.Invoke();
            return true;
        }
        return false;
    }

    public IEnumerator<UserSettingDto> GetEnumerator()
    {
        UserSettingData[] view;
        lock (_lock)
            view = [.. _settings];

        foreach (var settingData in view)
        {
            yield return UserSettingDto.Map(settingData);
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
