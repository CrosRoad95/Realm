using System.ComponentModel;
using static System.Net.Mime.MediaTypeNames;

namespace Realm.Persistance;

public abstract class Db<T> : IdentityDbContext<User, Role, Guid,
        IdentityUserClaim<Guid>,
        IdentityUserRole<Guid>,
        IdentityUserLogin<Guid>,
        IdentityRoleClaim<Guid>,
        IdentityUserToken<Guid>>, IDb where T : Db<T>
{
    public DbSet<UserData> UserData => Set<UserData>();

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
    }
}