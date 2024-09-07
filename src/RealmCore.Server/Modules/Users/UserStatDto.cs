namespace RealmCore.Server.Modules.Users;

public sealed class UserStatDto
{
    public required int StatId { get; init; }
    public required float Value { get; init; }

    public UserStatDto() { }

    [SetsRequiredMembers]
    public UserStatDto(int statId, float value)
    {
        StatId = statId;
        Value = value;
    }

    [return: NotNullIfNotNull(nameof(userStatData))]
    public static UserStatDto? Map(UserStatisticData? userStatData)
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
