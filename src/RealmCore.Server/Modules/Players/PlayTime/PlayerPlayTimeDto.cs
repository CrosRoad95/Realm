namespace RealmCore.Server.Modules.Players.PlayTime;

public sealed class PlayerPlayTimeDto : IEquatable<PlayerPlayTimeDto>
{
    public required int Category { get; init; }
    public required TimeSpan PlayTime { get; init; }

    [SetsRequiredMembers]
    public PlayerPlayTimeDto(int category, TimeSpan playTime)
    {
        Category = category;
        PlayTime = playTime;
    }

    [return: NotNullIfNotNull(nameof(userPlayTimeData))]
    public static PlayerPlayTimeDto? Map(UserPlayTimeData? userPlayTimeData)
    {
        if (userPlayTimeData == null)
            return null;

        return new PlayerPlayTimeDto(userPlayTimeData.Category, TimeSpan.FromSeconds(userPlayTimeData.PlayTime));
    }

    public bool Equals(PlayerPlayTimeDto? other)
    {
        if (other == null)
            return false;

        return other.Category == Category;
    }
}
