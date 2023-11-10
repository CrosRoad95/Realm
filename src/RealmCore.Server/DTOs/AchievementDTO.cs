namespace RealmCore.Server.DTOs;

public class AchievementDTO
{
    public int AchievementId { get; set; }
    public float Progress { get; set; }
    public string? Value { get; set; }
    public bool PrizeReceived { get; set; }

    [return: NotNullIfNotNull(nameof(achievementData))]
    public static AchievementDTO? Map(AchievementData? achievementData)
    {
        if (achievementData == null)
            return null;

        return new AchievementDTO
        {
            AchievementId = achievementData.AchievementId,
            PrizeReceived = achievementData.PrizeReceived,
            Progress = achievementData.Progress,
            Value = achievementData.Value
        };
    }
}
