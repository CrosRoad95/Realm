namespace RealmCore.Server.Modules.Players.Settings;

public class SettingNotFoundException : Exception
{
    public SettingNotFoundException(int settingId)
    {
        SettingId = settingId;
    }

    public int SettingId { get; }
}
