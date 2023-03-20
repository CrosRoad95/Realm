namespace Realm.Persistance.Data;

public sealed class User : IdentityUser<int>
{
#pragma warning disable CS8618
    public string? Nick { get; set; }
    public DateTime? RegisteredDateTime { get; set; }
    public DateTime? LastLogindDateTime { get; set; }
    public string? RegisterSerial { get; set; }
    public string? RegisterIp { get; set; }
    public string? LastSerial { get; set; }
    public string? LastIp { get; set; }
    public ulong PlayTime { get; set; }
    public short Skin { get; set; }
    public decimal Money { get; set; }
    public uint Level { get; set; }
    public uint Experience { get; set; }

    public TransformAndMotion? LastTransformAndMotion { get; set; } = null;
#pragma warning restore CS8618

    public ICollection<UserLicense> Licenses { get; set; } = new List<UserLicense>();
    public ICollection<VehicleAccess> VehicleAccesses { get; set; } = new List<VehicleAccess>();
    public ICollection<Inventory>? Inventories { get; set; } = new List<Inventory>();
    public DailyVisits? DailyVisits { get; set; }
    public ICollection<Achievement> Achievements { get; set; } = new List<Achievement>();
    public ICollection<JobUpgrade> JobUpgrades { get; set; } = new List<JobUpgrade>();
    public ICollection<JobStatistics> JobStatistics { get; set; } = new List<JobStatistics>();
    public ICollection<Discovery> Discoveries { get; set; } = new List<Discovery>();
    public ICollection<GroupMember> GroupMembers { get; set; } = new List<GroupMember>();
    public ICollection<FractionMember> FractionMembers { get; set; } = new List<FractionMember>();
    public DiscordIntegration? DiscordIntegration { get; set; }
    public ICollection<UserUpgrade> Upgrades { get; set; } = new List<UserUpgrade>();
    public ICollection<UserStat> Stats { get; set; } = new List<UserStat>();
    public ICollection<UserReward> Rewards { get; set; } = new List<UserReward>();
    public ICollection<UserSetting> Settings { get; set; } = new List<UserSetting>();
    public ICollection<UserWhitelistedSerial> WhitelistedSerials { get; set; } = new List<UserWhitelistedSerial>();
}
