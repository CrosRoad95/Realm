using Realm.Persistance.Interfaces;
using static Realm.Server.Interfaces.IJobService;
using JobStatistics = Realm.Server.Interfaces.IJobService.JobStatistics;

namespace Realm.Server.Services;

internal class JobService : IJobService
{
    private readonly IJobRepository _jobRepository;

    public JobService(IJobRepository jobRepository)
    {
        _jobRepository = jobRepository;
    }

    public async Task<UserJobStatistics> GetTotalUserJobStatistics(int userId, int jobId)
    {
        var query = _jobRepository.GetAll().Where(x => x.UserId == userId && x.JobId == jobId);
        var jobStatistics = await query
            .GroupBy(x => true)
            .Select(x => new
            {
                points = x.Sum(y => (int)y.Points),
                timePlayed = x.Sum(y => (int)y.TimePlayed)
            })
            .FirstOrDefaultAsync() ?? throw new Exception("User and job id not found.");
        return new UserJobStatistics
        {
            points = jobStatistics.points,
            timePlayed = jobStatistics.timePlayed
        };
    }

    public async Task<List<JobStatistics>> GetTotalJobStatistics(int jobId)
    {
        var query = _jobRepository.GetAll()
            .Where(x => x.JobId == jobId)
            .GroupBy(x => x.UserId)
            .Select(x => new JobStatistics
            {
                userId = x.Key,
                points = x.Sum(y => (int)y.Points),
                timePlayed = x.Sum(y => (int)y.TimePlayed)
            });

        return await query.ToListAsync();
    }
}
