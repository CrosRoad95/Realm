using Realm.Persistance.Interfaces;
using JobStatistics = Realm.Server.Interfaces.IJobService.JobStatistics;

namespace Realm.Server.Services;

internal class JobService : IJobService
{
    private readonly IJobRepository _jobRepository;

    public JobService(IJobRepository jobRepository)
    {
        _jobRepository = jobRepository;
    }

    public async Task<JobStatistics?> TryGetTotalUserJobStatistics(int userId, short jobId)
    {
        var queryResult = await _jobRepository.GetUserJobStatistics(userId, jobId);
        if(queryResult != null)
        {
            return new JobStatistics
            {
                points = queryResult.Points,
                timePlayed = queryResult.TimePlayed
            };
        }
        return null;
    }

    public async Task<Dictionary<int, JobStatistics>> GetTotalJobStatistics(short jobId, int limit = 10)
    {
        var results = await _jobRepository.GetJobStatistics(jobId, limit);

        return results.ToDictionary(x => x.Key, x => new JobStatistics
        {
            points = x.Value.Points,
            timePlayed = x.Value.TimePlayed
        });
    }
}
