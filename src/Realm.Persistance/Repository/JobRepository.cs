namespace Realm.Persistance.Repository;

internal class JobRepository : IJobRepository
{
    private readonly IDb _db;

    public JobRepository(IDb db)
    {
        _db = db;
    }

    public IQueryable<JobStatistics> GetAll()
    {
        return _db.JobPoints;
    }

    public async Task Commit()
    {
        await _db.SaveChangesAsync();
    }

    public void Dispose()
    {
        _db.Dispose();
    }
}
