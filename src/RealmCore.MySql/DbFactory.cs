﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System.Diagnostics.CodeAnalysis;

namespace RealmCore.MySql;

[ExcludeFromCodeCoverage]
public sealed class DbFactory : IDesignTimeDbContextFactory<MySqlDb>
{
    public MySqlDb CreateDbContext(string[] args)
    {
        var connectionString = "server=localhost;port=3306;database=realm;uid=root;password=password";

        var builder = new DbContextOptionsBuilder<MySqlDb>();
        builder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
        return new MySqlDb(builder.Options);
    }
}