using Realm.Persistance.Data.Helpers;

namespace Realm.Persistance;

public abstract class Db<T> : IdentityDbContext<User, Role, Guid,
        IdentityUserClaim<Guid>,
        IdentityUserRole<Guid>,
        IdentityUserLogin<Guid>,
        IdentityRoleClaim<Guid>,
        IdentityUserToken<Guid>>, IDb where T : Db<T>
{
    public DbSet<UserData> UserData => Set<UserData>();
    public DbSet<UserLicense> UserLicenses => Set<UserLicense>();
    public DbSet<Vehicle> Vehicles => Set<Vehicle>();
    public DbSet<VehicleData> VehicleData => Set<VehicleData>();

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
            entityBuilder.Property(x => x.Components)
                .HasDefaultValue(null)
                .HasMaxLength(262140)
                .IsRequired(false);
            entityBuilder.Property(x => x.Inventory)
                .HasDefaultValue(null)
                .HasMaxLength(262140)
                .IsRequired(false);
        });

        modelBuilder.Entity<UserData>(entityBuilder =>
        {
            entityBuilder
                .ToTable("UserData")
                .HasKey(x => new { x.UserId, x.Key });
            entityBuilder
                .HasOne(x => x.User)
                .WithMany(x => x.PlayerData)
                .HasForeignKey(x => x.UserId);
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

            entityBuilder.Property(x => x.Components)
                .HasDefaultValue(null)
                .HasMaxLength(65535)
                .IsRequired(false);
        });

        modelBuilder.Entity<VehicleData>(entityBuilder =>
        {
            entityBuilder
                .ToTable("VehicleData")
                .HasKey(x => new { x.VehicleId, x.Key });

            entityBuilder
                .HasOne(x => x.Vehicle)
                .WithMany(x => x.VehicleData)
                .HasForeignKey(x => x.VehicleId);
        });
    }
}