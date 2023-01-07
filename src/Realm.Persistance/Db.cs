using Realm.Persistance.Data;

namespace Realm.Persistance;

public abstract class Db<T> : IdentityDbContext<User, Role, Guid,
        IdentityUserClaim<Guid>,
        IdentityUserRole<Guid>,
        IdentityUserLogin<Guid>,
        IdentityRoleClaim<Guid>,
        IdentityUserToken<Guid>>, IDb where T : Db<T>
{
    public DbSet<UserLicense> UserLicenses => Set<UserLicense>();
    public DbSet<Vehicle> Vehicles => Set<Vehicle>();
    public DbSet<Inventory> Inventories => Set<Inventory>();
    public DbSet<InventoryItem> InventoryItems => Set<InventoryItem>();
    public DbSet<VehicleUpgrade> VehicleUpgrades => Set<VehicleUpgrade>();
    public DbSet<VehicleFuel> VehicleFuels => Set<VehicleFuel>();

    public Db(DbContextOptions<T> options) : base(options)
    {
        Database.Migrate();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>().ToTable("Users");
        modelBuilder.Entity<Role>().ToTable("Roles");
        modelBuilder.Entity<IdentityUserClaim<Guid>>().ToTable("UserClaims");
        modelBuilder.Entity<IdentityUserRole<Guid>>().ToTable("UserRoles");
        modelBuilder.Entity<IdentityUserLogin<Guid>>().ToTable("UserLogins");
        modelBuilder.Entity<IdentityRoleClaim<Guid>>().ToTable("RoleClaims");
        modelBuilder.Entity<IdentityUserToken<Guid>>().ToTable("UserTokens");

        modelBuilder.Entity<User>(entityBuilder =>
        {
            entityBuilder.Property(x => x.LastTransformAndMotion)
                .HasMaxLength(400)
                .HasConversion(x => x.Serialize(), x => TransformAndMotion.CreateFromString(x))
                .HasDefaultValue(null);

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
                .HasForeignKey(x => x.UserId);

            entityBuilder
                .HasOne(x => x.DailyVisits)
                .WithOne(x => x.User)
                .HasForeignKey<DailyVisits>(x => x.Id);

            entityBuilder
                .HasOne(x => x.Statistics)
                .WithOne(x => x.User)
                .HasForeignKey<Statistics>(x => x.Id);
        });

        modelBuilder.Entity<Inventory>(entityBuilder =>
        {
            entityBuilder
                .ToTable("Inventories")
                .HasKey(x => x.Id);

            entityBuilder
                .HasMany(x => x.InventoryItems)
                .WithOne(x => x.Inventory)
                .HasForeignKey(x => x.InventoryId)
                .OnDelete(DeleteBehavior.Cascade);

            entityBuilder.Property(x => x.Id)
                .HasMaxLength(36);
        });
        
        modelBuilder.Entity<DailyVisits>(entityBuilder =>
        {
            entityBuilder
                .ToTable("DailyVisits")
                .HasKey(x => x.Id);

            entityBuilder.Property(x => x.LastVisit)
                .HasDefaultValue(DateTime.MinValue);
        });
        
        modelBuilder.Entity<Statistics>(entityBuilder =>
        {
            entityBuilder
                .ToTable("Statistics")
                .HasKey(x => x.Id);
        });
        
        modelBuilder.Entity<Achievement>(entityBuilder =>
        {
            entityBuilder
                .ToTable("Achievements")
                .HasKey(x => new { x.UserId, x.Name });

            entityBuilder.Property(x => x.Value)
                .HasMaxLength(255);
        });
        
        modelBuilder.Entity<JobUpgrade>(entityBuilder =>
        {
            entityBuilder
                .ToTable("JobUpgrades")
                .HasKey(x => new { x.UserId, x.JobId, x.Name });

            entityBuilder.Property(x => x.Name)
                .HasMaxLength(255);
        });
        
        modelBuilder.Entity<InventoryItem>(entityBuilder =>
        {
            entityBuilder
                .ToTable("InventoryItems")
                .HasKey(x => new { x.Id, x.InventoryId });

            entityBuilder.Property(x => x.Id)
                .HasMaxLength(36);

            entityBuilder.Property(x => x.InventoryId)
                .HasMaxLength(36);
        });

        modelBuilder.Entity<UserLicense>(entityBuilder =>
        {
            entityBuilder
                .ToTable("UserLicense")
                .HasKey(x => new { x.UserId, x.LicenseId });
            entityBuilder
                .HasOne(x => x.User)
                .WithMany(x => x.Licenses)
                .HasForeignKey(x => x.UserId);
        });
        

        modelBuilder.Entity<VehicleUpgrade>(entityBuilder =>
        {
            entityBuilder
                .ToTable("VehicleUpgrades")
                .HasKey(x => new { x.VehicleId, x.UpgradeId });
        });

        modelBuilder.Entity<Vehicle>(entityBuilder =>
        {
            entityBuilder.ToTable("Vehicles")
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
        });

        modelBuilder.Entity<VehicleAccess>(entityBuilder =>
        {
            entityBuilder
                .ToTable("VehicleAccesses")
                .HasKey(x => x.Id);

            entityBuilder
                .Property(x => x.Id)
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

    }
}