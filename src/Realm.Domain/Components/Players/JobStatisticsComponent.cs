using JobStatisticsData = Realm.Persistance.Data.JobStatistics;
using JobStatistics = Realm.Domain.Concepts.JobStatistics;

namespace Realm.Domain.Components.Players;

[ComponentUsage(false)]
public class JobStatisticsComponent : Component
{
    private readonly Dictionary<short, JobStatistics> _jobStatistics = new();
    public IReadOnlyDictionary<short, JobStatistics> JobStatistics => _jobStatistics;

    private readonly DateOnly _date = DateOnly.FromDateTime(DateTime.Now);
    internal DateOnly Date => _date;
    private readonly object _lock = new object();

    public JobStatisticsComponent()
    {

    }

    internal JobStatisticsComponent(ICollection<JobStatisticsData> jobStatistics)
    {
        _jobStatistics = jobStatistics.GroupBy(x => x.JobId).ToDictionary(x => x.Key, x => new JobStatistics
        {
            jobId = x.Key,
            points= (ulong)x.Sum(x => (decimal)x.Points),
            timePlayed = (ulong)x.Sum(x => (decimal)x.TimePlayed),
            sessionPoints = 0,
            sessionTimePlayed = 0,
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
        lock(_lock)
        {
            EnsureJobIsInitialized(jobId);
            var jobStatistics = _jobStatistics[jobId];
            jobStatistics.points += points;
            jobStatistics.sessionPoints += points;
            _jobStatistics[jobId] = jobStatistics;
        }
    }

    public void AddTimePlayed(short jobId, ulong timePlayed)
    {
        lock (_lock)
        {
            EnsureJobIsInitialized(jobId);
            var jobStatistics = _jobStatistics[jobId];
            jobStatistics.timePlayed += timePlayed;
            jobStatistics.sessionTimePlayed += timePlayed;
            _jobStatistics[jobId] = jobStatistics;
        }
    }

    internal void Reset()
    {
        lock (_lock)
        {
            foreach (var item in _jobStatistics.Keys)
            {
                var jobStatistics = _jobStatistics[item];
                jobStatistics.sessionPoints = 0;
                jobStatistics.sessionTimePlayed = 0;
                _jobStatistics[item] = jobStatistics;
            }
        }
    }
}
