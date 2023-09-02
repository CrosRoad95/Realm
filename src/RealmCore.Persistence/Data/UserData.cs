using RealmCore.Persistence.Data.Helpers;

namespace RealmCore.Persistence.Data;

public sealed class UserData : IdentityUser<int>
{
    public string? Nick { get; set; }
    public DateTime? RegisteredDateTime { get; set; }
    public DateTime? LastLoginDateTime { get; set; }
    public string? RegisterSerial { get; set; }
    public string? RegisterIp { get; set; }
    public string? LastSerial { get; set; }
    public string? LastIp { get; set; }
    public ulong PlayTime { get; set; }
    public short Skin { get; set; }
    public decimal Money { get; set; }
    public uint Level { get; set; }
    public uint Experience { get; set; }
    public bool IsDisabled { get; set; }
    public bool QuickLogin { get; set; }
    public TransformAndMotion? LastTransformAndMotion { get; set; } = null;

    public ICollection<UserLicenseData> Licenses { get; set; } = new List<UserLicenseData>();
    public ICollection<VehicleUserAccessData> VehicleUserAccesses { get; set; } = new List<VehicleUserAccessData>();
    public ICollection<InventoryData> Inventories { get; set; } = new List<InventoryData>();
    public DailyVisitsData? DailyVisits { get; set; }
    public ICollection<AchievementData> Achievements { get; set; } = new List<AchievementData>();
    public ICollection<JobUpgradeData> JobUpgrades { get; set; } = new List<JobUpgradeData>();
    public ICollection<JobStatisticsData> JobStatistics { get; set; } = new List<JobStatisticsData>();
    public ICollection<DiscoveryData> Discoveries { get; set; } = new List<DiscoveryData>();
    public ICollection<GroupMemberData> GroupMembers { get; set; } = new List<GroupMemberData>();
    public ICollection<FractionMemberData> FractionMembers { get; set; } = new List<FractionMemberData>();
    public DiscordIntegrationData? DiscordIntegration { get; set; }
    public ICollection<UserUpgradeData> Upgrades { get; set; } = new List<UserUpgradeData>();
    public ICollection<UserStatData> Stats { get; set; } = new List<UserStatData>();
    public ICollection<UserRewardData> Rewards { get; set; } = new List<UserRewardData>();
    public ICollection<UserSettingData> Settings { get; set; } = new List<UserSettingData>();
    public ICollection<UserWhitelistedSerialData> WhitelistedSerials { get; set; } = new List<UserWhitelistedSerialData>();
    public ICollection<UserInventoryData> UserInventories = new List<UserInventoryData>();
    public ICollection<RatingData> Ratings = new List<RatingData>();
    public ICollection<OpinionData> Opinions = new List<OpinionData>();
}
