using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System.Diagnostics.CodeAnalysis;

namespace RealmCore.MySql;

[ExcludeFromCodeCoverage]
public sealed class DbFactory : IDesignTimeDbContextFactory<MySqlDb>
{
    public DbFactory()
    {
    }

    public MySqlDb CreateDbContext(string[] args)
    {
        var builder = new DbContextOptionsBuilder<MySqlDb>();
        builder.UseMySql(args[0], ServerVersion.AutoDetect(args[0]));
        return new MySqlDb(builder.Options);
    }
}