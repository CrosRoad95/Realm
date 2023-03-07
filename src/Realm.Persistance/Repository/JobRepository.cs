namespace Realm.Persistance.Repository;

internal class JobRepository : IJobRepository
{
    private readonly IDb _db;

    public JobRepository(IDb db)
    {
        _db = db;
    }
    
    public Task<Dictionary<int, PlayerJobStatisticsDTO>> GetJobStatistics(short jobId, int limit = 10)
    {
        var query = _db.JobPoints
            .TagWithSource(nameof(JobRepository))
            .AsNoTrackingWithIdentityResolution()
            .Where(x => x.JobId == jobId)
            .GroupBy(x => x.UserId)
            .Select(x => new PlayerJobStatisticsDTO
            {
                UserId = x.Key,
                Points = x.Sum(y => (int)y.Points),
                TimePlayed = x.Sum(y => (int)y.TimePlayed)
            })
            .OrderBy(x => x.Points)
            .Take(limit);

        return query.ToDictionaryAsync(x => x.UserId);
    }
    
    public Task<JobStatisticsDTO?> GetPlayerJobStatistics(int userId, short jobId)
    {
        var query = _db.JobPoints
            .TagWithSource(nameof(JobRepository))
            .AsNoTrackingWithIdentityResolution()
            .Where(x => x.JobId == jobId)
            .GroupBy(x => true)
            .Select(x => new JobStatisticsDTO
            {
                Points = x.Sum(y => (int)y.Points),
                TimePlayed = x.Sum(y => (int)y.TimePlayed)
            });

        return query.FirstOrDefaultAsync();
    }

    public Task Commit()
    {
        return _db.SaveChangesAsync();
    }

    public void Dispose()
    {
        _db.Dispose();
    }
}
