using RealmCore.Persistence.Data;

namespace RealmCore.Server.Modules.Players.Jobs;

public record struct JobStatisticsSummary(ulong points, ulong timePlayed);

public sealed class PlayerJobStatisticsFeature : IPlayerFeature, IEnumerable<UserJobStatisticsDto>, IUsesUserPersistentData
{
    private readonly object _lock = new();
    private ICollection<JobStatisticsData> _jobStatistics = [];
    private readonly IDateTimeProvider _dateTimeProvider;

    public event Action<PlayerJobStatisticsFeature, short, ulong>? PointsAdded;
    public event Action<PlayerJobStatisticsFeature, short, ulong>? TimePlayedAdded;
    public event Action? VersionIncreased;

    public RealmPlayer Player { get; init; }

    public PlayerJobStatisticsFeature(PlayerContext playerContext, IDateTimeProvider dateTimeProvider)
    {
        Player = playerContext.Player;
        _dateTimeProvider = dateTimeProvider;
    }

    public void LogIn(UserData userData)
    {
        lock (_lock)
            _jobStatistics = userData.JobStatistics;
    }

    private JobStatisticsData? GetJobStatisticsData(short jobId)
    {
        JobStatisticsData? jobStatisticsData;
        var today = DateOnly.FromDateTime(_dateTimeProvider.Now);
        lock (_lock)
        {
            jobStatisticsData = _jobStatistics.Where(x => x.JobId == jobId && x.Date == today).FirstOrDefault();
            if (jobStatisticsData == null)
            {
                jobStatisticsData = new JobStatisticsData
                {
                    JobId = jobId,
                    Date = today,
                };
                _jobStatistics.Add(jobStatisticsData);
            }

        }
        VersionIncreased?.Invoke();
        return jobStatisticsData;
    }

    public bool AddPoints(short jobId, ulong points)
    {
        ArgumentOutOfRangeException.ThrowIfZero(points);

        JobStatisticsData? jobStatisticsData;

        lock (_lock)
        {
            jobStatisticsData = GetJobStatisticsData(jobId);
            if (jobStatisticsData == null)
                return false;
            jobStatisticsData.Points += points;
        }

        VersionIncreased?.Invoke();
        PointsAdded?.Invoke(this, jobId, jobStatisticsData.Points);
        return true;
    }

    public bool AddTimePlayed(short jobId, ulong timePlayed)
    {
        ArgumentOutOfRangeException.ThrowIfZero(timePlayed);

        JobStatisticsData? jobStatisticsData;

        lock (_lock)
        {
            jobStatisticsData = GetJobStatisticsData(jobId);
            if (jobStatisticsData == null)
                return false;

            jobStatisticsData.TimePlayed += timePlayed;
        }
        VersionIncreased?.Invoke();
        TimePlayedAdded?.Invoke(this, jobId, jobStatisticsData.TimePlayed);
        return true;
    }

    public bool AddPointsAndTimePlayed(short jobId, ulong points, ulong timePlayed)
    {
        ArgumentOutOfRangeException.ThrowIfZero(points);
        ArgumentOutOfRangeException.ThrowIfZero(timePlayed);

        JobStatisticsData? jobStatisticsData;

        lock (_lock)
        {
            jobStatisticsData = GetJobStatisticsData(jobId);
            if (jobStatisticsData == null)
                return false;

            jobStatisticsData.Points += points;
            jobStatisticsData.TimePlayed += timePlayed;
        }

        VersionIncreased?.Invoke();
        PointsAdded?.Invoke(this, jobId, jobStatisticsData.Points);
        TimePlayedAdded?.Invoke(this, jobId, jobStatisticsData.TimePlayed);
        return true;
    }

    public JobStatisticsSummary GetSummary(short jobId)
    {
        ulong totalPoints = 0;
        ulong totalTimePlayed = 0;
        lock (_lock)
        {
            foreach (var jobStatistic in _jobStatistics)
            {
                if (jobStatistic.JobId == jobId)
                {
                    totalPoints += jobStatistic.Points;
                    totalTimePlayed += jobStatistic.TimePlayed;
                }
            }
        }
        return new JobStatisticsSummary(totalPoints, totalTimePlayed);
    }

    public IEnumerator<UserJobStatisticsDto> GetEnumerator()
    {
        JobStatisticsData[] view;
        {
            lock (_lock)
                view = [.. _jobStatistics];
        }

        foreach (var jobUpgradeData in view)
        {
            yield return UserJobStatisticsDto.Map(jobUpgradeData);
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
