using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using RealmCore.Configuration;
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
        var realmConfigurationProvider = new RealmConfiguration();
        var connectionString = realmConfigurationProvider.Get<string>("Database:ConnectionString");

        var builder = new DbContextOptionsBuilder<MySqlDb>();
        builder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
        return new MySqlDb(builder.Options);
    }
}