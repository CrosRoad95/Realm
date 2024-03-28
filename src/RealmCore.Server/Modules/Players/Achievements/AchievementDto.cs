namespace RealmCore.Server.Modules.Players.Achievements;

public sealed class AchievementDto : IEquatable<AchievementDto>
{
    public required int AchievementId { get; init; }
    public required float Progress { get; init; }
    public required string? Value { get; init; }
    public required DateTime? PrizeReceivedDateTime { get; init; }

    [return: NotNullIfNotNull(nameof(achievementData))]
    public static AchievementDto? Map(AchievementData? achievementData)
    {
        if (achievementData == null)
            return null;

        return new AchievementDto
        {
            AchievementId = achievementData.AchievementId,
            PrizeReceivedDateTime = achievementData.PrizeReceivedDateTime,
            Progress = achievementData.Progress,
            Value = achievementData.Value
        };
    }

    public bool Equals(AchievementDto? other)
    {
        if (other == null)
            return false;

        return other.AchievementId == AchievementId;
    }
}
