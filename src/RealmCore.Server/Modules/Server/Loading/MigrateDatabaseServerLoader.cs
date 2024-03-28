namespace RealmCore.Server.Modules.Server.Loading;

internal sealed class MigrateDatabaseServerLoader : IServerLoader
{
    private readonly IDb _db;

    public MigrateDatabaseServerLoader(IDb db)
    {
        _db = db;
    }

    public async Task Load()
    {
        await _db.MigrateAsync();
    }
}
