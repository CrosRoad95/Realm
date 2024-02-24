namespace RealmCore.Persistence.Repository;

public interface IJobRepository
{
    Task<List<UserJobStatisticsDto>> GetJobStatistics(short jobId, int limit = 10, CancellationToken cancellationToken = default);
    Task<UserJobStatisticsDto?> GetUserJobStatistics(int userId, short jobId, CancellationToken cancellationToken = default);
}

internal sealed class JobRepository : IJobRepository
{
    private readonly IDb _db;

    public JobRepository(IDb db)
    {
        _db = db;
    }

    public async Task<List<UserJobStatisticsDto>> GetJobStatistics(short jobId, int limit = 10, CancellationToken cancellationToken = default)
    {
        var query = _db.JobPoints
            .TagWithSource(nameof(JobRepository))
            .AsNoTrackingWithIdentityResolution()
            .Where(x => x.JobId == jobId)
            .GroupBy(x => x.UserId)
            .Select(x => new UserJobStatisticsDto
            {
                UserId = x.Key,
                JobId = jobId,
                Points = (ulong)x.Sum(y => (uint)y.Points),
                TimePlayed = (ulong)x.Sum(y => (uint)y.TimePlayed)
            })
            .OrderBy(x => x.Points)
            .Take(limit);

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<UserJobStatisticsDto?> GetUserJobStatistics(int userId, short jobId, CancellationToken cancellationToken = default)
    {
        var query = _db.JobPoints
            .TagWithSource(nameof(JobRepository))
            .AsNoTrackingWithIdentityResolution()
            .Where(x => x.JobId == jobId)
            .GroupBy(x => true)
            .Select(x => new UserJobStatisticsDto
            {
                JobId = jobId,
                UserId = userId,
                Points = (ulong)x.Sum(y => (uint)y.Points),
                TimePlayed = (ulong)x.Sum(y => (uint)y.TimePlayed)
            });

        return await query.FirstOrDefaultAsync(cancellationToken);
    }
}
