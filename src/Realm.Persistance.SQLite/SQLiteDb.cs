﻿namespace Realm.Persistance.SQLite;

public sealed class SQLiteDb : Db<SQLiteDb>
{
    public SQLiteDb(DbContextOptions<SQLiteDb> options) : base(options)
    {
    }
}