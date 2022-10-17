namespace Realm.Persistance;

public abstract class Db<T> : DbContext, IDb where T : Db<T>
{
    public DbSet<Test> Tests => Set<Test>();
    public DbSet<AdminGroup> AdminGroups => Set<AdminGroup>();
    public DbSet<UserAccount> UserAccounts => Set<UserAccount>();

    public Db(DbContextOptions<T> options) : base(options)
    {
        Database.Migrate();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Test>(entityBuilder =>
        {
            entityBuilder.HasKey(x => x.Id);
        });

        modelBuilder.Entity<AdminGroup>(entityBuilder =>
        {
            entityBuilder.HasKey(x => x.Id);
            entityBuilder.HasMany(x => x.Users);
        });

        modelBuilder.Entity<UserAccount>(entityBuilder =>
        {
            entityBuilder.HasKey(x => x.Id);
            entityBuilder.HasIndex(x => x.Login);
            entityBuilder.HasMany(x => x.AdminGroups);
        });
    }
}