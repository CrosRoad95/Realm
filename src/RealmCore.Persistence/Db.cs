﻿using RealmCore.Persistence.Data.Helpers;
using System.Diagnostics.CodeAnalysis;

namespace RealmCore.Persistence;

[ExcludeFromCodeCoverage]
public abstract class Db<T> : IdentityDbContext<UserData, RoleData, int,
        IdentityUserClaim<int>,
        IdentityUserRole<int>,
        IdentityUserLogin<int>,
        IdentityRoleClaim<int>,
        IdentityUserToken<int>>, IDb where T : Db<T>
{
    public DbSet<UserLicenseData> UserLicenses => Set<UserLicenseData>();
    public DbSet<VehicleData> Vehicles => Set<VehicleData>();
    public DbSet<VehicleUserAccessData> VehicleUserAccess => Set<VehicleUserAccessData>();
    public DbSet<InventoryData> Inventories => Set<InventoryData>();
    public DbSet<InventoryItemData> InventoryItems => Set<InventoryItemData>();
    public DbSet<VehicleUpgradeData> VehicleUpgrades => Set<VehicleUpgradeData>();
    public DbSet<VehicleFuelData> VehicleFuels => Set<VehicleFuelData>();
    public DbSet<DailyVisitsData> DailyVisits => Set<DailyVisitsData>();
    public DbSet<UserStatData> UserStats => Set<UserStatData>();
    public DbSet<JobStatisticsData> JobPoints => Set<JobStatisticsData>();
    public DbSet<JobUpgradeData> JobUpgrades => Set<JobUpgradeData>();
    public DbSet<AchievementData> Achievements => Set<AchievementData>();
    public DbSet<DiscoveryData> Discoveries => Set<DiscoveryData>();
    public DbSet<GroupData> Groups => Set<GroupData>();
    public DbSet<GroupMemberData> GroupMembers => Set<GroupMemberData>();
    public DbSet<FractionData> Fractions => Set<FractionData>();
    public DbSet<FractionMemberData> FractionMembers => Set<FractionMemberData>();
    public DbSet<DiscordIntegrationData> DiscordIntegrations => Set<DiscordIntegrationData>();
    public DbSet<UserUpgradeData> UserUpgrades => Set<UserUpgradeData>();
    public DbSet<VehiclePartDamageData> VehiclePartDamages => Set<VehiclePartDamageData>();
    public DbSet<BanData> Bans => Set<BanData>();
    public DbSet<UserRewardData> UserRewards => Set<UserRewardData>();
    public DbSet<UserSettingData> UserSettings => Set<UserSettingData>();
    public DbSet<UserWhitelistedSerialData> UserWhitelistedSerials => Set<UserWhitelistedSerialData>();
    public DbSet<VehicleEngineData> VehicleEngines => Set<VehicleEngineData>();
    public DbSet<UserInventoryData> UserInventories => Set<UserInventoryData>();
    public DbSet<VehicleInventoryData> VehicleInventories => Set<VehicleInventoryData>();
    public DbSet<VehicleEventData> VehicleEvents => Set<VehicleEventData>();
    public DbSet<RatingData> Ratings => Set<RatingData>();
    public DbSet<OpinionData> Opinions => Set<OpinionData>();

    public Db(DbContextOptions<T> options) : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
#if DEBUG
        optionsBuilder.EnableSensitiveDataLogging();
#endif
    }

    public async Task MigrateAsync()
    {
        await Database.MigrateAsync();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<UserData>().ToTable("Users");
        modelBuilder.Entity<RoleData>().ToTable("Roles");
        modelBuilder.Entity<IdentityUserClaim<int>>().ToTable("UserClaims");
        modelBuilder.Entity<IdentityUserRole<int>>().ToTable("UserRoles");
        modelBuilder.Entity<IdentityUserLogin<int>>().ToTable("UserLogins");
        modelBuilder.Entity<IdentityRoleClaim<int>>().ToTable("RoleClaims");
        modelBuilder.Entity<IdentityUserToken<int>>().ToTable("UserTokens");

        modelBuilder.Entity<UserData>(entityBuilder =>
        {
            entityBuilder.Property(x => x.LastTransformAndMotion)
                .HasMaxLength(400)
                .HasConversion(x => x.Serialize(), x => TransformAndMotion.CreateFromString(x))
                .HasDefaultValue(null);

            entityBuilder
                .HasMany(x => x.Licenses)
                .WithOne()
                .HasForeignKey(x => x.UserId);

            entityBuilder
                .HasMany(x => x.VehicleUserAccesses)
                .WithOne(x => x.User)
                .HasForeignKey(x => x.UserId);

            entityBuilder
                .HasMany(x => x.Achievements)
                .WithOne()
                .HasForeignKey(x => x.UserId);

            entityBuilder
                .HasMany(x => x.JobUpgrades)
                .WithOne()
                .HasForeignKey(x => x.UserId)
                .HasPrincipalKey(x => x.Id);

            entityBuilder
                .HasMany(x => x.JobStatistics)
                .WithOne()
                .HasForeignKey(x => x.UserId);

            entityBuilder
                .HasOne(x => x.DailyVisits)
                .WithOne()
                .HasForeignKey<DailyVisitsData>(x => x.UserId);

            entityBuilder
                .HasMany(x => x.Stats)
                .WithOne()
                .HasForeignKey(x => x.UserId);
            //.OnDelete(DeleteBehavior.Cascade);

            entityBuilder
                .HasMany(x => x.Rewards)
                .WithOne()
                .HasForeignKey(x => x.UserId);

            entityBuilder
                .HasMany(x => x.Settings)
                .WithOne()
                .HasForeignKey(x => x.UserId);

            entityBuilder
                .HasMany(x => x.WhitelistedSerials)
                .WithOne()
                .HasForeignKey(x => x.UserId);

            entityBuilder
                .HasMany(x => x.Inventories)
                .WithMany()
                .UsingEntity<UserInventoryData>(y => y
                    .HasOne(z => z.Inventory)
                    .WithMany(z => z.UserInventories)
                    .HasForeignKey(z => z.InventoryId),
                    y => y
                    .HasOne(z => z.User)
                    .WithMany(z => z.UserInventories)
                    .HasForeignKey(z => z.UserId));

            entityBuilder
                .HasMany(x => x.Discoveries)
                .WithOne()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entityBuilder
                .HasMany(x => x.GroupMembers)
                .WithOne(x => x.User)
                .HasForeignKey(x => x.UserId)
                .HasPrincipalKey(x => x.Id)
                .OnDelete(DeleteBehavior.Cascade);

            entityBuilder
                .HasMany(x => x.FractionMembers)
                .WithOne(x => x.User)
                .HasForeignKey(x => x.UserId)
                .HasPrincipalKey(x => x.Id)
                .OnDelete(DeleteBehavior.Cascade);

            entityBuilder
                .HasMany(x => x.Upgrades)
                .WithOne()
                .HasForeignKey(x => x.UserId)
                .HasPrincipalKey(x => x.Id);

            entityBuilder
                .HasOne(x => x.DiscordIntegration)
                .WithOne(x => x.User)
                .HasForeignKey<DiscordIntegrationData>(x => x.UserId);

            entityBuilder
                .HasMany(x => x.Ratings)
                .WithOne(x => x.User)
                .HasForeignKey(x => x.UserId);

            entityBuilder
                .HasMany(x => x.Opinions)
                .WithOne(x => x.User)
                .HasForeignKey(x => x.UserId);

            entityBuilder.Property(x => x.IsDisabled)
                .HasDefaultValue(false)
                .IsRequired();
        });

        modelBuilder.Entity<InventoryData>(entityBuilder =>
        {
            entityBuilder
                .ToTable(nameof(Inventories))
                .HasKey(x => x.Id);

            entityBuilder.Property(x => x.Size)
                .HasPrecision(38, 18);

            entityBuilder
                .HasMany(x => x.InventoryItems)
                .WithOne(x => x.Inventory)
                .HasForeignKey(x => x.InventoryId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<UserInventoryData>(entityBuilder =>
        {
            entityBuilder
                .ToTable(nameof(UserInventoryData))
                .HasKey(x => new { x.UserId, x.InventoryId });
        });

        modelBuilder.Entity<VehicleInventoryData>(entityBuilder =>
        {
            entityBuilder
                .ToTable(nameof(VehicleInventories))
                .HasKey(x => new { x.VehicleId, x.InventoryId });
        });


        modelBuilder.Entity<VehicleEventData>(entityBuilder =>
        {
            entityBuilder
                .ToTable(nameof(VehicleEvents))
                .HasKey(x => x.Id);
        });

        modelBuilder.Entity<DailyVisitsData>(entityBuilder =>
        {
            entityBuilder
                .ToTable(nameof(DailyVisits))
                .HasKey(x => x.UserId);

            entityBuilder.Property(x => x.LastVisit)
                .HasDefaultValue(DateTime.MinValue);
        });

        modelBuilder.Entity<UserStatData>(entityBuilder =>
        {
            entityBuilder
                .ToTable(nameof(UserStats))
                .HasKey(x => new { x.UserId, x.StatId });
        });

        modelBuilder.Entity<UserRewardData>(entityBuilder =>
        {
            entityBuilder
                .ToTable(nameof(UserRewards))
                .HasKey(x => new { x.UserId, x.RewardId });
        });

        modelBuilder.Entity<UserSettingData>(entityBuilder =>
        {
            entityBuilder
                .ToTable(nameof(UserSettings))
                .HasKey(x => new { x.UserId, x.SettingId });

            entityBuilder.Property(x => x.SettingId)
                .HasMaxLength(255);
        });

        modelBuilder.Entity<UserWhitelistedSerialData>(entityBuilder =>
        {
            entityBuilder
                .ToTable(nameof(UserWhitelistedSerials))
                .HasKey(x => new { x.UserId, x.Serial });

            entityBuilder.Property(x => x.Serial)
                .HasMaxLength(32);
        });

        modelBuilder.Entity<VehicleEngineData>(entityBuilder =>
        {
            entityBuilder
                .ToTable(nameof(VehicleEngines))
                .HasKey(x => new { x.VehicleId, x.EngineId });
        });

        modelBuilder.Entity<AchievementData>(entityBuilder =>
        {
            entityBuilder
                .ToTable(nameof(Achievements))
                .HasKey(x => new { x.UserId, x.AchievementId });

            entityBuilder.Property(x => x.Value)
                .HasMaxLength(255);
        });

        modelBuilder.Entity<JobUpgradeData>(entityBuilder =>
        {
            entityBuilder
                .ToTable(nameof(JobUpgrades))
                .HasKey(x => x.Id);

            entityBuilder
                .ToTable(nameof(JobUpgrades))
                .HasIndex(x => new { x.UserId, x.JobId, x.UpgradeId });
        });

        modelBuilder.Entity<JobStatisticsData>(entityBuilder =>
        {
            entityBuilder
                .ToTable(nameof(JobPoints))
                .HasKey(x => new { x.UserId, x.JobId, x.Date });

            entityBuilder.Property(x => x.Date)
                .HasColumnType("date");
        });

        modelBuilder.Entity<InventoryItemData>(entityBuilder =>
        {
            entityBuilder
                .ToTable(nameof(InventoryItems))
                .HasKey(x => new { x.Id, x.InventoryId });

            entityBuilder.Property(x => x.Id)
                .HasMaxLength(36);
        });

        modelBuilder.Entity<UserLicenseData>(entityBuilder =>
        {
            entityBuilder
                .ToTable(nameof(UserLicenses))
                .HasKey(x => new { x.UserId, x.LicenseId });
        });


        modelBuilder.Entity<VehicleUpgradeData>(entityBuilder =>
        {
            entityBuilder
                .ToTable(nameof(VehicleUpgrades))
                .HasKey(x => new { x.VehicleId, x.UpgradeId });
        });

        modelBuilder.Entity<VehicleData>(entityBuilder =>
        {
            entityBuilder.ToTable(nameof(Vehicles))
                .HasKey(x => x.Id);

            entityBuilder.Property(x => x.Platetext)
                .HasMaxLength(32)
                .IsRequired();

            entityBuilder.Property(x => x.TransformAndMotion)
                .HasMaxLength(400)
                .HasConversion(x => x.Serialize(), x => TransformAndMotion.CreateFromString(x))
                .HasDefaultValue(new TransformAndMotion())
                .IsRequired();

            entityBuilder.Property(x => x.Color)
                .HasMaxLength(300)
                .HasConversion(x => x.Serialize(), x => VehicleColor.CreateFromString(x))
                .HasDefaultValue(new VehicleColor())
                .IsRequired();

            entityBuilder.Property(x => x.Paintjob)
                .HasDefaultValue(3)
                .IsRequired();

            entityBuilder.Property(x => x.Variant)
                .HasMaxLength(50)
                .HasConversion(x => x.Serialize(), x => VehicleVariant.CreateFromString(x))
                .HasDefaultValue(new VehicleVariant())
                .IsRequired();

            entityBuilder.Property(x => x.DamageState)
                .HasMaxLength(300)
                .HasConversion(x => x.Serialize(), x => VehicleDamageState.CreateFromString(x))
                .HasDefaultValue(new VehicleDamageState())
                .IsRequired();

            entityBuilder.Property(x => x.DoorOpenRatio)
                .HasMaxLength(200)
                .HasConversion(x => x.Serialize(), x => VehicleDoorOpenRatio.CreateFromString(x))
                .HasDefaultValue(new VehicleDoorOpenRatio())
                .IsRequired();

            entityBuilder.Property(x => x.WheelStatus)
                .HasMaxLength(200)
                .HasConversion(x => x.Serialize(), x => VehicleWheelStatus.CreateFromString(x))
                .HasDefaultValue(new VehicleWheelStatus())
                .IsRequired();

            entityBuilder.Property(x => x.EngineState)
                .HasDefaultValue(false)
                .IsRequired();

            entityBuilder.Property(x => x.LandingGearDown)
                .HasDefaultValue(true)
                .IsRequired();

            entityBuilder.Property(x => x.OverrideLights)
                .HasDefaultValue(false)
                .IsRequired();

            entityBuilder.Property(x => x.SirensState)
                .HasDefaultValue(false)
                .IsRequired();

            entityBuilder.Property(x => x.Locked)
                .HasDefaultValue(false)
                .IsRequired();

            entityBuilder.Property(x => x.TaxiLightState)
                .HasDefaultValue(false)
                .IsRequired();

            entityBuilder.Property(x => x.Health)
                .HasDefaultValue(1000)
                .IsRequired();

            entityBuilder.Property(x => x.IsFrozen)
                .HasDefaultValue(false)
                .IsRequired();

            entityBuilder.Property(x => x.Removed)
                .HasDefaultValue(false)
                .IsRequired();

            entityBuilder.Property(x => x.Spawned)
                .HasDefaultValue(false)
                .IsRequired();
            
            entityBuilder.Property(x => x.IsRemoved)
                .HasDefaultValue(false)
                .IsRequired();

            entityBuilder
                .HasMany(x => x.UserAccesses)
                .WithOne(x => x.Vehicle)
                .HasForeignKey(x => x.VehicleId);

            entityBuilder
                .HasMany(x => x.Upgrades)
                .WithOne()
                .HasForeignKey(x => x.VehicleId);

            entityBuilder
                .HasMany(x => x.Fuels)
                .WithOne()
                .HasForeignKey(x => x.VehicleId);

            entityBuilder
                .HasMany(x => x.PartDamages)
                .WithOne()
                .HasForeignKey(x => x.VehicleId);

            entityBuilder
                .HasMany(x => x.VehicleEngines)
                .WithOne()
                .HasForeignKey(x => x.VehicleId);

            entityBuilder
                .HasMany(x => x.Inventories)
                .WithMany()
                .UsingEntity<VehicleInventoryData>(y => y
                    .HasOne(z => z.Inventory)
                    .WithMany(z => z.VehicleInventories)
                    .HasForeignKey(z => z.InventoryId),
                    y => y
                    .HasOne(z => z.Vehicle)
                    .WithMany(z => z.VehicleInventories)
                    .HasForeignKey(z => z.VehicleId));

            entityBuilder
                .HasMany(x => x.VehicleEvents)
                .WithOne()
                .HasForeignKey(x => x.VehicleId);

        });

        modelBuilder.Entity<VehicleUserAccessData>(entityBuilder =>
        {
            entityBuilder
                .ToTable(nameof(VehicleUserAccess))
                .HasKey(x => x.Id);

            entityBuilder
                .Property(x => x.VehicleId)
                .ValueGeneratedOnAdd();

            entityBuilder
                .Property(x => x.CustomValue)
                .HasMaxLength(255);

            entityBuilder
                .HasOne(x => x.User)
                .WithMany(x => x.VehicleUserAccesses)
                .HasForeignKey(x => x.UserId);
        });

        modelBuilder.Entity<VehicleFuelData>(entityBuilder =>
        {
            entityBuilder
                .ToTable(nameof(VehicleFuels))
                .HasKey(x => new { x.VehicleId, x.FuelType });

            entityBuilder.Property(x => x.FuelType)
                .HasMaxLength(16);
        });


        modelBuilder.Entity<VehiclePartDamageData>(entityBuilder =>
        {
            entityBuilder
                .ToTable(nameof(VehiclePartDamages))
                .HasKey(x => new { x.VehicleId, x.PartId });
        });

        modelBuilder.Entity<DiscoveryData>(entityBuilder =>
        {
            entityBuilder
                .ToTable(nameof(Discoveries))
                .HasKey(x => new { x.UserId, x.DiscoveryId });
        });

        modelBuilder.Entity<GroupData>(entityBuilder =>
        {
            entityBuilder
                .ToTable(nameof(Groups))
                .HasKey(x => x.Id);

            entityBuilder.HasIndex(x => x.Name).IsUnique();
            entityBuilder.HasIndex(x => x.Shortcut).IsUnique();

            entityBuilder.Property(x => x.Name)
                .HasMaxLength(128);

            entityBuilder.Property(x => x.Shortcut)
                .HasMaxLength(8);

            entityBuilder.HasMany(x => x.Members)
                .WithOne(x => x.Group)
                .HasForeignKey(x => x.GroupId)
                .HasPrincipalKey(x => x.Id);
        });

        modelBuilder.Entity<GroupMemberData>(entityBuilder =>
        {
            entityBuilder
                .ToTable(nameof(GroupMembers))
                .HasKey(x => new { x.GroupId, x.UserId });

            entityBuilder.Property(x => x.RankName)
                .HasMaxLength(128);

            entityBuilder.Property(x => x.RankName)
                .HasMaxLength(64);

            entityBuilder.HasOne(x => x.Group)
                .WithMany(x => x.Members)
                .HasForeignKey(x => x.GroupId)
                .HasPrincipalKey(x => x.Id);
        });

        modelBuilder.Entity<FractionData>(entityBuilder =>
        {
            entityBuilder
                .ToTable(nameof(Fractions))
                .HasKey(x => x.Id);

            entityBuilder.HasIndex(x => x.Name).IsUnique();
            entityBuilder.HasIndex(x => x.Code).IsUnique();

            entityBuilder.Property(x => x.Name)
                .HasMaxLength(128);

            entityBuilder.Property(x => x.Code)
                .HasMaxLength(8);

            entityBuilder.HasMany(x => x.Members)
                .WithOne(x => x.Fraction)
                .HasForeignKey(x => x.FractionId)
                .HasPrincipalKey(x => x.Id);
        });

        modelBuilder.Entity<FractionMemberData>(entityBuilder =>
        {
            entityBuilder
                .ToTable(nameof(FractionMembers))
                .HasKey(x => new { x.FractionId, x.UserId });

            entityBuilder.Property(x => x.RankName)
                .HasMaxLength(128);

            entityBuilder.Property(x => x.RankName)
                .HasMaxLength(64);

            entityBuilder.HasOne(x => x.Fraction)
                .WithMany(x => x.Members)
                .HasForeignKey(x => x.FractionId)
                .HasPrincipalKey(x => x.Id);
        });

        modelBuilder.Entity<DiscordIntegrationData>(entityBuilder =>
        {
            entityBuilder
                .ToTable(nameof(DiscordIntegrations))
                .HasKey(x => x.UserId);
        });

        modelBuilder.Entity<UserUpgradeData>(entityBuilder =>
        {
            entityBuilder
                .ToTable(nameof(UserUpgrades))
                .HasKey(x => new { x.UserId, x.UpgradeId });
        });

        modelBuilder.Entity<BanData>(entityBuilder =>
        {
            entityBuilder
                .ToTable(nameof(Bans))
                .HasKey(x => x.Id);

            entityBuilder.Property(x => x.Serial)
                .HasMaxLength(32);

            entityBuilder.Property(x => x.Reason)
                .HasMaxLength(256);
        });

        modelBuilder.Entity<UserInventoryData>(entityBuilder =>
        {
            entityBuilder
                .ToTable(nameof(UserInventories))
                .HasKey(x => new { x.UserId, x.InventoryId });
        });

        modelBuilder.Entity<VehicleInventoryData>(entityBuilder =>
        {
            entityBuilder
                .ToTable(nameof(VehicleInventories))
                .HasKey(x => new { x.VehicleId, x.InventoryId });
        });

        modelBuilder.Entity<RatingData>(entityBuilder =>
        {
            entityBuilder
                .ToTable(nameof(Ratings))
                .HasKey(x => x.Id);
        });

        modelBuilder.Entity<OpinionData>(entityBuilder =>
        {
            entityBuilder
                .ToTable(nameof(Opinions))
                .HasKey(x => x.Id);
        });
    }
}