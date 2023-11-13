namespace RealmCore.Server.DTOs;

public class AchievementDTO
{
    public int AchievementId { get; set; }
    public float Progress { get; set; }
    public string? Value { get; set; }
    public DateTime? PrizeReceivedDateTime { get; set; }

    [return: NotNullIfNotNull(nameof(achievementData))]
    public static AchievementDTO? Map(AchievementData? achievementData)
    {
        if (achievementData == null)
            return null;

        return new AchievementDTO
        {
            AchievementId = achievementData.AchievementId,
            PrizeReceivedDateTime = achievementData.PrizeReceivedDateTime,
            Progress = achievementData.Progress,
            Value = achievementData.Value
        };
    }
}
