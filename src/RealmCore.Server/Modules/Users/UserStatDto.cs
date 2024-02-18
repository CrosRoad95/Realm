namespace RealmCore.Server.Modules.Users;

public class UserStatDto
{
    public int StatId { get; set; }
    public float Value { get; set; }

    public UserStatDto() { }

    public UserStatDto(int statId, float value)
    {
        StatId = statId;
        Value = value;
    }

    [return: NotNullIfNotNull(nameof(userStatData))]
    public static UserStatDto? Map(UserStatData? userStatData)
    {
        if (userStatData == null)
            return null;

        return new UserStatDto
        {
            StatId = userStatData.StatId,
            Value = userStatData.Value,
        };
    }

}
