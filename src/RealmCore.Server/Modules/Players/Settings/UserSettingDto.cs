namespace RealmCore.Server.Modules.Players.Settings;

public sealed class UserSettingDto : IEquatable<UserSettingDto>
{
    public int SettingId { get; init; }
    public string Value { get; init; }

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

    public bool Equals(UserSettingDto? other)
    {
        if (other == null)
            return false;

        return other.SettingId == SettingId;
    }
}
