namespace RealmCore.Server.Services;

internal sealed class JobService : IJobService
{
    private readonly IJobRepository _jobRepository;

    public JobService(IJobRepository jobRepository)
    {
        _jobRepository = jobRepository;
    }

    public async Task<JobStatistics?> TryGetTotalUserJobStatistics(int userId, short jobId)
    {
        var queryResult = await _jobRepository.GetUserJobStatistics(userId, jobId);
        if (queryResult != null)
        {
            return new JobStatistics
            {
                points = (ulong)queryResult.Points,
                timePlayed = (ulong)queryResult.TimePlayed
            };
        }
        return null;
    }

    public async Task<Dictionary<int, JobStatistics>> GetTotalJobStatistics(short jobId, int limit = 10)
    {
        var results = await _jobRepository.GetJobStatistics(jobId, limit);

        return results.ToDictionary(x => x.Key, x => new JobStatistics
        {
            points = (ulong)x.Value.Points,
            timePlayed = (ulong)x.Value.TimePlayed
        });
    }
}
