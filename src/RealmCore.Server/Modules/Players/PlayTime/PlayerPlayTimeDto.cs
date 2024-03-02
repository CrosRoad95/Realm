namespace RealmCore.Server.Modules.Players.PlayTime;

public sealed class PlayerPlayTimeDto : IEquatable<PlayerPlayTimeDto>
{
    public int Category { get; set; }
    public TimeSpan PlayTime { get; set; }

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
