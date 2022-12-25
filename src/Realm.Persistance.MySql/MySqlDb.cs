using Microsoft.EntityFrameworkCore;

namespace Realm.Persistance.MySql;

public sealed class MySqlDb : Db<MySqlDb>
{
    public MySqlDb(DbContextOptions<MySqlDb> options) : base(options)
    {
    }
}