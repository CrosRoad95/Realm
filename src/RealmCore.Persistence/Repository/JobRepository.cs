namespace RealmCore.Persistence.Repository;

public interface IJobRepository
{
    Task<UserJobStatisticsDto[]> GetJobStatistics(short jobId, int limit = 10, CancellationToken cancellationToken = default);
    Task<UserJobStatisticsDto?> GetUserJobStatistics(int userId, short jobId, CancellationToken cancellationToken = default);
}

internal sealed class JobRepository : IJobRepository
{
    private readonly IDb _db;

    public JobRepository(IDb db)
    {
        _db = db;
    }

    public async Task<UserJobStatisticsDto[]> GetJobStatistics(short jobId, int limit = 10, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(GetJobStatistics));

        if (activity != null)
        {
            activity.AddTag("JobId", jobId);
            activity.AddTag("Limit", limit);
        }

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
            .OrderByDescending(x => x.Points)
            .Take(limit);

        return await query.ToArrayAsync(cancellationToken);
    }

    public async Task<UserJobStatisticsDto?> GetUserJobStatistics(int userId, short jobId, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(GetUserJobStatistics));

        if (activity != null)
        {
            activity.AddTag("JobId", jobId);
            activity.AddTag("JobId", jobId);
        }

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

    public static readonly ActivitySource Activity = new("RealmCore.JobRepository", "1.0.0");
}
