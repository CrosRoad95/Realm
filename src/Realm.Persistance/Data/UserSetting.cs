namespace Realm.Persistance.Data;

public sealed class UserSetting
{
    public int UserId { get; set; }
    public int SettingId { get; set; }
    public string Value { get; set; }
}
