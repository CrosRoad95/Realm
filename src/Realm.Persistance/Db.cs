namespace Realm.Persistance;

public abstract class Db<T> : IdentityDbContext<User, Role, int,
        IdentityUserClaim<int>,
        IdentityUserRole<int>,
        IdentityUserLogin<int>,
        IdentityRoleClaim<int>,
        IdentityUserToken<int>>, IDb where T : Db<T>
{
    public DbSet<UserLicense> UserLicenses => Set<UserLicense>();
    public DbSet<Vehicle> Vehicles => Set<Vehicle>();
    public DbSet<VehicleAccess> VehicleAccesses => Set<VehicleAccess>();
    public DbSet<Inventory> Inventories => Set<Inventory>();
    public DbSet<InventoryItem> InventoryItems => Set<InventoryItem>();
    public DbSet<VehicleUpgrade> VehicleUpgrades => Set<VehicleUpgrade>();
    public DbSet<VehicleFuel> VehicleFuels => Set<VehicleFuel>();
    public DbSet<DailyVisits> DailyVisits => Set<DailyVisits>();
    public DbSet<Statistics> Statistics => Set<Statistics>();
    public DbSet<JobStatistics> JobPoints => Set<JobStatistics>();
    public DbSet<JobUpgrade> JobUpgrades => Set<JobUpgrade>();
    public DbSet<Achievement> Achievements => Set<Achievement>();
    public DbSet<Discovery> Discoveries => Set<Discovery>();
    public DbSet<Group> Groups => Set<Group>();
    public DbSet<GroupMember> GroupMembers => Set<GroupMember>();
    public DbSet<Fraction> Fractions => Set<Fraction>();
    public DbSet<FractionMember> FractionMembers => Set<FractionMember>();
    public DbSet<DiscordIntegration> DiscordIntegrations => Set<DiscordIntegration>();
    public DbSet<UserUpgrade> UserUpgrades => Set<UserUpgrade>();
    public DbSet<VehiclePartDamage> VehiclePartDamages => Set<VehiclePartDamage>();

    public Db(DbContextOptions<T> options) : base(options)
    {
    }

    public async Task MigrateAsync()
    {
        await Database.MigrateAsync();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>().ToTable("Users");
        modelBuilder.Entity<Role>().ToTable("Roles");
        modelBuilder.Entity<IdentityUserClaim<int>>().ToTable("UserClaims");
        modelBuilder.Entity<IdentityUserRole<int>>().ToTable("UserRoles");
        modelBuilder.Entity<IdentityUserLogin<int>>().ToTable("UserLogins");
        modelBuilder.Entity<IdentityRoleClaim<int>>().ToTable("RoleClaims");
        modelBuilder.Entity<IdentityUserToken<int>>().ToTable("UserTokens");

        modelBuilder.Entity<User>(entityBuilder =>
        {
            entityBuilder.Property(x => x.LastTransformAndMotion)
                .HasMaxLength(400)
                .HasConversion(x => x.Serialize(), x => TransformAndMotion.CreateFromString(x))
                .HasDefaultValue(null);

            entityBuilder
                .HasMany(x => x.Licenses)
                .WithOne(x => x.User)
                .HasForeignKey(x => x.UserId);

            entityBuilder
                .HasMany(x => x.VehicleAccesses)
                .WithOne(x => x.User)
                .HasForeignKey(x => x.UserId);

            entityBuilder
                .HasMany(x => x.Achievements)
                .WithOne(x => x.User)
                .HasForeignKey(x => x.UserId);

            entityBuilder
                .HasMany(x => x.JobUpgrades)
                .WithOne(x => x.User)
                .HasForeignKey(x => x.UserId)
                .HasPrincipalKey(x => x.Id);
            
            entityBuilder
                .HasMany(x => x.JobStatistics)
                .WithOne(x => x.User)
                .HasForeignKey(x => x.UserId);

            entityBuilder
                .HasOne(x => x.DailyVisits)
                .WithOne(x => x.User)
                .HasForeignKey<DailyVisits>(x => x.UserId);

            entityBuilder
                .HasOne(x => x.Statistics)
                .WithOne(x => x.User)
                .HasForeignKey<Statistics>(x => x.UserId);
                //.OnDelete(DeleteBehavior.Cascade);

            entityBuilder
                .HasMany(x => x.Inventories)
                .WithOne(x => x.User)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entityBuilder
                .HasMany(x => x.Discoveries)
                .WithOne(x => x.User)
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
                .HasMany(x => x.UserUpgrades)
                .WithOne(x => x.User)
                .HasForeignKey(x => x.UserId)
                .HasPrincipalKey(x => x.Id)
                .OnDelete(DeleteBehavior.Cascade);

            entityBuilder
                .HasOne(x => x.DiscordIntegration)
                .WithOne(x => x.User)
                .HasForeignKey<DiscordIntegration>(x => x.UserId);
        });

        modelBuilder.Entity<Inventory>(entityBuilder =>
        {
            entityBuilder
                .ToTable(nameof(Inventories))
                .HasKey(x => x.Id);

            entityBuilder
                .HasMany(x => x.InventoryItems)
                .WithOne(x => x.Inventory)
                .HasForeignKey(x => x.InventoryId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        
        modelBuilder.Entity<DailyVisits>(entityBuilder =>
        {
            entityBuilder
                .ToTable(nameof(DailyVisits))
                .HasKey(x => x.UserId);

            entityBuilder.Property(x => x.LastVisit)
                .HasDefaultValue(DateTime.MinValue);
        });
        
        modelBuilder.Entity<Statistics>(entityBuilder =>
        {
            entityBuilder
                .ToTable(nameof(Statistics))
                .HasKey(x => x.UserId);
        });
        
        modelBuilder.Entity<Achievement>(entityBuilder =>
        {
            entityBuilder
                .ToTable(nameof(Achievements))
                .HasKey(x => new { x.UserId, x.Name });

            entityBuilder.Property(x => x.Value)
                .HasMaxLength(255);
        });
        
        modelBuilder.Entity<JobUpgrade>(entityBuilder =>
        {
            entityBuilder
                .ToTable(nameof(JobUpgrades))
                .HasKey(x => x.Id);

            entityBuilder
                .ToTable(nameof(JobUpgrades))
                .HasIndex(x => new { x.UserId, x.JobId, x.UpgradeId });
        });
        
        modelBuilder.Entity<JobStatistics>(entityBuilder =>
        {
            entityBuilder
                .ToTable(nameof(JobPoints))
                .HasKey(x => new { x.UserId, x.JobId });
        });
        
        modelBuilder.Entity<InventoryItem>(entityBuilder =>
        {
            entityBuilder
                .ToTable(nameof(InventoryItems))
                .HasKey(x => new { x.Id, x.InventoryId });

            entityBuilder.Property(x => x.Id)
                .HasMaxLength(36);
        });

        modelBuilder.Entity<UserLicense>(entityBuilder =>
        {
            entityBuilder
                .ToTable(nameof(UserLicenses))
                .HasKey(x => new { x.UserId, x.LicenseId });
        });
        

        modelBuilder.Entity<VehicleUpgrade>(entityBuilder =>
        {
            entityBuilder
                .ToTable(nameof(VehicleUpgrades))
                .HasKey(x => new { x.VehicleId, x.UpgradeId });
        });

        modelBuilder.Entity<Vehicle>(entityBuilder =>
        {
            entityBuilder.ToTable(nameof(Vehicles))
                .HasKey(x =>  x.Id);

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

            entityBuilder
                .HasMany(x => x.VehicleAccesses)
                .WithOne(x => x.Vehicle)
                .HasForeignKey(x => x.VehicleId);

            entityBuilder
                .HasMany(x => x.Upgrades)
                .WithOne(x => x.Vehicle)
                .HasForeignKey(x => x.VehicleId);

            entityBuilder
                .HasMany(x => x.Fuels)
                .WithOne(x => x.Vehicle)
                .HasForeignKey(x => x.VehicleId);

            entityBuilder
                .HasMany(x => x.PartDamages)
                .WithOne(x => x.Vehicle)
                .HasForeignKey(x => x.VehicleId);
        });

        modelBuilder.Entity<VehicleAccess>(entityBuilder =>
        {
            entityBuilder
                .ToTable(nameof(VehicleAccesses))
                .HasKey(x => x.Id);

            entityBuilder
                .Property(x => x.VehicleId)
                .ValueGeneratedOnAdd();

            entityBuilder.Property(x => x.Description)
                .HasConversion(x => x.Serialize(), x => VehicleAccessDescription.CreateFromString(x))
                .HasDefaultValue(new VehicleAccessDescription())
                .IsRequired();

            entityBuilder
                .HasOne(x => x.User)
                .WithMany(x => x.VehicleAccesses)
                .HasForeignKey(x => x.UserId);

            entityBuilder
                .HasOne(x => x.Vehicle)
                .WithMany(x => x.VehicleAccesses)
                .HasForeignKey(x => x.VehicleId);
        });

        modelBuilder.Entity<VehicleFuel>(entityBuilder =>
        {
            entityBuilder
                .ToTable(nameof(VehicleFuels))
                .HasKey(x => new { x.VehicleId, x.FuelType });

            entityBuilder.Property(x => x.FuelType)
                .HasMaxLength(16);
        });
        

        modelBuilder.Entity<VehiclePartDamage>(entityBuilder =>
        {
            entityBuilder
                .ToTable(nameof(VehiclePartDamages))
                .HasKey(x => new { x.VehicleId, x.PartId });
        });
        
        modelBuilder.Entity<Discovery>(entityBuilder =>
        {
            entityBuilder
                .ToTable(nameof(Discoveries))
                .HasKey(x => new { x.UserId, x.DiscoveryId });
        });
        
        modelBuilder.Entity<Group>(entityBuilder =>
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
        
        modelBuilder.Entity<GroupMember>(entityBuilder =>
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
        
        modelBuilder.Entity<Fraction>(entityBuilder =>
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
        
        modelBuilder.Entity<FractionMember>(entityBuilder =>
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

        modelBuilder.Entity<DiscordIntegration>(entityBuilder =>
        {
            entityBuilder
                .ToTable(nameof(DiscordIntegration))
                .HasKey(x => x.UserId);
        });

        modelBuilder.Entity<UserUpgrade>(entityBuilder =>
        {
            entityBuilder
                .ToTable(nameof(UserUpgrade))
                .HasKey(x => new { x.UserId, x.UpgradeId });
        });
    }
}