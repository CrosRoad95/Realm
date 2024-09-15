namespace RealmCore.Server.Modules.Players.Settings;

public sealed class SettingDto : IEquatable<SettingDto>
{
    public required int SettingId { get; init; }
    public required string Value { get; init; }

    [return: NotNullIfNotNull(nameof(settingBaseData))]
    public static SettingDto? Map(SettingDataBase? settingBaseData)
    {
        if (settingBaseData == null)
            return null;

        return new SettingDto
        {
            SettingId = settingBaseData.SettingId,
            Value = settingBaseData.Value
        };
    }

    public bool Equals(SettingDto? other)
    {
        if (other == null)
            return false;

        return other.SettingId == SettingId;
    }
}
