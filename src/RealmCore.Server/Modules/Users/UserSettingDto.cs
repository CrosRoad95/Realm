namespace RealmCore.Server.Modules.Users;

public class UserSettingDto
{
    public int SettingId { get; set; }
    public string Value { get; set; }

    [return: NotNullIfNotNull(nameof(userSettingData))]
    public static UserSettingDto? Map(UserSettingData? userSettingData)
    {
        if (userSettingData == null)
            return null;

        return new UserSettingDto
        {
            SettingId = userSettingData.SettingId,
            Value = userSettingData.Value
        };
    }

}
