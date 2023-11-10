namespace RealmCore.Server.DTOs;

public class UserSettingDTO
{
    public int SettingId { get; set; }
    public string Value { get; set; }

    [return: NotNullIfNotNull(nameof(userSettingData))]
    public static UserSettingDTO? Map(UserSettingData? userSettingData)
    {
        if (userSettingData == null)
            return null;

        return new UserSettingDTO
        {
            SettingId = userSettingData.SettingId,
            Value = userSettingData.Value
        };
    }

}
