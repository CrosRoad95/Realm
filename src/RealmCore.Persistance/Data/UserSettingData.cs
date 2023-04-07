namespace RealmCore.Persistance.Data;

public sealed class UserSettingData
{
    public int UserId { get; set; }
    public int SettingId { get; set; }
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public string Value { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
}
