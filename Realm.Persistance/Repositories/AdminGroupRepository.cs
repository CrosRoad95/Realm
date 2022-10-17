namespace Realm.Persistance.Repositories;

internal class AdminGroupRepository : IAdminGroupRepository
{
    private readonly IDb _db;
    private readonly DbSet<AdminGroup> _set;

    public AdminGroupRepository(IDb db)
    {
        _db = db;
        _set = _db.AdminGroups;
    }

    public async Task<AdminGroup?> GetById(Guid id) => await _set.FirstOrDefaultAsync(x => x.Id == id);
}
