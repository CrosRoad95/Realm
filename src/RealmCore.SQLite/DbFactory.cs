namespace RealmCore.SQLite;

public class DbFactory : IDesignTimeDbContextFactory<SQLiteDb>
{
    public SQLiteDb CreateDbContext(string[] args)
    {
        var builder = new DbContextOptionsBuilder<SQLiteDb>();
        builder.UseSqlite("Filename=./server.db");
        return new SQLiteDb(builder.Options);
    }
}