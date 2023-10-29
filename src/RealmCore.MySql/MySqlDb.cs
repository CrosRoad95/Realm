using Microsoft.EntityFrameworkCore;
using RealmCore.Persistence;
using System.Diagnostics.CodeAnalysis;

namespace RealmCore.MySql;

[ExcludeFromCodeCoverage]
public sealed class MySqlDb : Db<MySqlDb>
{
    public MySqlDb(DbContextOptions<MySqlDb> options) : base(options)
    {
    }

    public override void Dispose()
    {
        base.Dispose();
    }
}