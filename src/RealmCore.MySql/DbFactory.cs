using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using RealmCore.Configuration;
using System.Diagnostics.CodeAnalysis;

namespace RealmCore.MySql;

[ExcludeFromCodeCoverage]
public sealed class DbFactory : IDesignTimeDbContextFactory<MySqlDb>
{
    private readonly IRealmConfigurationProvider _realmConfigurationProvider;

    public DbFactory(IRealmConfigurationProvider realmConfigurationProvider)
    {
        _realmConfigurationProvider = realmConfigurationProvider;
    }

    public MySqlDb CreateDbContext(string[] args)
    {
        var connectionString = _realmConfigurationProvider.Get<string>("Database:ConnectionString");

        var builder = new DbContextOptionsBuilder<MySqlDb>();
        builder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
        return new MySqlDb(builder.Options);
    }
}