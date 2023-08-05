using RealmCore.Persistence;
using System.Diagnostics.CodeAnalysis;

namespace RealmCore.SQLite;

[ExcludeFromCodeCoverage]
public sealed class SQLiteDb : Db<SQLiteDb>
{
    public SQLiteDb(DbContextOptions<SQLiteDb> options) : base(options)
    {
    }
}
