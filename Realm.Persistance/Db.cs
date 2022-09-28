namespace Realm.Persistance;

public abstract class Db<T> : DbContext, IDb where T : Db<T>
{
    public DbSet<Test> Tests => Set<Test>();

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
    }
}