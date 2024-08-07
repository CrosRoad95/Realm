﻿namespace RealmCore.Persistence.Data;

public sealed class UserAchievementData
{
    public int UserId { get; set; }
    public int AchievementId { get; set; }
    public float Progress { get; set; }
    public string? Value { get; set; }
    public DateTime? PrizeReceivedDateTime { get; set; }
}
