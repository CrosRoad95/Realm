namespace RealmCore.Server.Modules.Users;

public sealed class UserSettingDto
{
    public required int SettingId { get; init; }
    public required string Value { get; init; }

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
