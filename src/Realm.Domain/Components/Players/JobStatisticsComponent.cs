using JobStatisticsData = Realm.Persistance.Data.JobStatistics;
using JobStatistics = Realm.Domain.Concepts.JobStatistics;

namespace Realm.Domain.Components.Players;

[ComponentUsage(false)]
public class JobStatisticsComponent : Component
{
    private readonly Dictionary<short, JobStatistics> _jobStatistics = new();
    public IReadOnlyDictionary<short, JobStatistics> JobStatistics => _jobStatistics;

    public event Action<Entity, short, JobStatistics>? StatisticsChanged;

    public JobStatisticsComponent()
    {

    }

    internal JobStatisticsComponent(ICollection<JobStatisticsData> jobStatistics)
    {
        _jobStatistics = jobStatistics.ToDictionary(x => x.JobId, x => new JobStatistics
        {
            jobId = x.JobId,
            points= x.Points,
            timePlayed = x.TimePlayed,
        });
    }

    private void EnsureJobIsInitialized(short jobId)
    {
        if (_jobStatistics.ContainsKey(jobId))
            return;
        _jobStatistics[jobId] = new JobStatistics
        {
            jobId = jobId,
        };
    }

    public void AddPoints(short jobId, ulong points)
    {
        EnsureJobIsInitialized(jobId);
        var jobStatistics = _jobStatistics[jobId];
        jobStatistics.points += points;
        _jobStatistics[jobId] = jobStatistics;

        StatisticsChanged?.Invoke(Entity, jobId, jobStatistics);
    }

    public void AddTimePlayed(short jobId, ulong timePlayed)
    {
        EnsureJobIsInitialized(jobId);
        var jobStatistics = _jobStatistics[jobId];
        jobStatistics.timePlayed += timePlayed;
        _jobStatistics[jobId] = jobStatistics;

        StatisticsChanged?.Invoke(Entity, jobId, jobStatistics);
    }
}
