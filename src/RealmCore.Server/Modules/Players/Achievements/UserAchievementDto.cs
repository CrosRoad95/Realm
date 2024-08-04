namespace RealmCore.Server.Modules.Players.Achievements;

public sealed class UserAchievementDto : IEquatable<UserAchievementDto>
{
    public required int AchievementId { get; init; }
    public required float Progress { get; init; }
    public required string? Value { get; init; }
    public required DateTime? PrizeReceivedDateTime { get; init; }

    [return: NotNullIfNotNull(nameof(achievementData))]
    public static UserAchievementDto? Map(UserAchievementData? achievementData)
    {
        if (achievementData == null)
            return null;

        return new UserAchievementDto
        {
            AchievementId = achievementData.AchievementId,
            PrizeReceivedDateTime = achievementData.PrizeReceivedDateTime,
            Progress = achievementData.Progress,
            Value = achievementData.Value
        };
    }

    public bool Equals(UserAchievementDto? other)
    {
        if (other == null)
            return false;

        return other.AchievementId == AchievementId;
    }
}
