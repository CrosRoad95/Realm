namespace RealmCore.Server.Modules.Elements;

public abstract class ElementSettingsFeature<T> : IEnumerable<SettingDto> where T: SettingDataBase, new()
{
    private readonly object _lock = new();
    private ICollection<T> _settings = [];

    public event Action<ElementSettingsFeature<T>, SettingDto>? Added;
    public event Action<ElementSettingsFeature<T>, SettingDto>? Changed;
    public event Action<ElementSettingsFeature<T>, SettingDto>? Removed;
    public event Action? VersionIncreased;

    public int[] SettingsIds
    {
        get
        {
            lock (_lock)
                return _settings.Select(x => x.SettingId).ToArray();
        }
    }

    public void Load(ICollection<T> settings)
    {
        foreach (var item in settings)
        {
            Set(item.SettingId, item.Value);
        }
        lock (_lock)
            _settings = settings;
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
        T? setting;
        lock (_lock)
        {
            setting = _settings.FirstOrDefault(x => x.SettingId == settingId);
            if (setting != null)
            {
                setting.Value = value;
            }
            else
            {
                setting = new T();

                setting.SettingId = settingId;
                setting.Value = value;

                _settings.Add(setting);
            }
        }

        if (added)
        {
            Added?.Invoke(this, SettingDto.Map(setting));
        }
        else
        {
            Changed?.Invoke(this, SettingDto.Map(setting));
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
        T? setting;
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
            Removed?.Invoke(this, SettingDto.Map(setting));
            VersionIncreased?.Invoke();
            return true;
        }
        return false;
    }

    public IEnumerator<SettingDto> GetEnumerator()
    {
        T[] view;
        lock (_lock)
            view = [.. _settings];

        foreach (var settingData in view)
        {
            yield return SettingDto.Map(settingData);
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
