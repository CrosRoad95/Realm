using RealmCore.Persistance.Data;

namespace RealmCore.Server.Components.Players;

[ComponentUsage(false)]
public class JobStatisticsComponent : Component
{
    private readonly Dictionary<short, JobStatistics> _jobStatistics = new();
    public IReadOnlyDictionary<short, JobStatistics> JobStatistics
    {
        get
        {
            ThrowIfDisposed();
            lock (_lock)
                return new Dictionary<short, JobStatistics>(_jobStatistics);
        }
    }

    private readonly DateOnly _date;
    internal DateOnly Date => _date;
    private readonly object _lock = new();

    public event Action<JobStatisticsComponent, short, ulong, ulong>? PointsAdded;
    public event Action<JobStatisticsComponent, short, ulong, ulong>? TimePlayedAdded;

    public JobStatisticsComponent(DateTime forDateTime)
    {
        _date = DateOnly.FromDateTime(forDateTime);
    }

    internal JobStatisticsComponent(DateTime forDateTime, ICollection<JobStatisticsData> jobStatistics)
    {
        _date = DateOnly.FromDateTime(forDateTime);
        _jobStatistics = jobStatistics.GroupBy(x => x.JobId).ToDictionary(x => x.Key, x => new JobStatistics
        {
            jobId = x.Key,
            points = (ulong)x.Sum(x => (decimal)x.Points),
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
        ThrowIfDisposed();

        lock (_lock)
        {
            EnsureJobIsInitialized(jobId);
            var jobStatistics = _jobStatistics[jobId];
            jobStatistics.points += points;
            jobStatistics.sessionPoints += points;
            _jobStatistics[jobId] = jobStatistics;
            PointsAdded?.Invoke(this, jobId, jobStatistics.points, jobStatistics.sessionPoints);
        }
    }

    public void AddTimePlayed(short jobId, ulong timePlayed)
    {
        ThrowIfDisposed();

        lock (_lock)
        {
            EnsureJobIsInitialized(jobId);
            var jobStatistics = _jobStatistics[jobId];
            jobStatistics.timePlayed += timePlayed;
            jobStatistics.sessionTimePlayed += timePlayed;
            _jobStatistics[jobId] = jobStatistics;
            TimePlayedAdded?.Invoke(this, jobId, jobStatistics.timePlayed, jobStatistics.sessionTimePlayed);
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
