using Microsoft.EntityFrameworkCore;
using RealmCore.Persistance;

namespace RealmCore.MySql;

public sealed class MySqlDb : Db<MySqlDb>
{
    public MySqlDb(DbContextOptions<MySqlDb> options) : base(options)
    {
    }
}