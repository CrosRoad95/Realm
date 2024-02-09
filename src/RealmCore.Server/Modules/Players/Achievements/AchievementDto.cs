namespace RealmCore.Server.Modules.Players.Achievements;

public class AchievementDto
{
    public int AchievementId { get; set; }
    public float Progress { get; set; }
    public string? Value { get; set; }
    public DateTime? PrizeReceivedDateTime { get; set; }

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
}
