namespace RealmCore.Server.Modules.Players.Jobs;

public record struct JobStatisticsSummary(ulong points, ulong timePlayed);

public interface IPlayerJobStatisticsFeature : IPlayerFeature, IEnumerable<UserJobStatisticsDto>
{
    void AddPoints(short jobId, ulong points);
    void AddPointsAndTimePlayed(short jobId, ulong points, ulong timePlayed);
    void AddTimePlayed(short jobId, ulong timePlayed);
    JobStatisticsSummary GetSummary(short jobId);
}

internal sealed class PlayerJobStatisticsFeature : IPlayerJobStatisticsFeature, IUsesUserPersistentData
{
    private readonly object _lock = new();
    private ICollection<JobStatisticsData> _jobStatistics = [];
    private readonly IDateTimeProvider _dateTimeProvider;

    public event Action<IPlayerJobStatisticsFeature, short, ulong>? PointsAdded;
    public event Action<IPlayerJobStatisticsFeature, short, ulong>? TimePlayedAdded;
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

    public void LogOut()
    {
        lock (_lock)
            _jobStatistics = [];
    }

    private JobStatisticsData GetJobStatisticsData(short jobId)
    {
        var today = DateOnly.FromDateTime(_dateTimeProvider.Now);
        lock (_lock)
        {
            var jobStatisticsData = _jobStatistics.Where(x => x.JobId == jobId && x.Date == today).FirstOrDefault();
            if (jobStatisticsData == null)
            {
                jobStatisticsData = new JobStatisticsData
                {
                    JobId = jobId,
                    Date = today,
                };
                VersionIncreased?.Invoke();
                _jobStatistics.Add(jobStatisticsData);
            }

            return jobStatisticsData;
        }
    }

    public void AddPoints(short jobId, ulong points)
    {
        ArgumentOutOfRangeException.ThrowIfZero(points);

        lock (_lock)
        {
            var jobStatisticsData = GetJobStatisticsData(jobId);
            jobStatisticsData.Points += points;
            VersionIncreased?.Invoke();
            PointsAdded?.Invoke(this, jobId, jobStatisticsData.Points);
        }
    }

    public void AddTimePlayed(short jobId, ulong timePlayed)
    {
        ArgumentOutOfRangeException.ThrowIfZero(timePlayed);

        lock (_lock)
        {
            var jobStatisticsData = GetJobStatisticsData(jobId);
            jobStatisticsData.TimePlayed += timePlayed;
            VersionIncreased?.Invoke();
            TimePlayedAdded?.Invoke(this, jobId, jobStatisticsData.TimePlayed);
        }
    }

    public void AddPointsAndTimePlayed(short jobId, ulong points, ulong timePlayed)
    {
        ArgumentOutOfRangeException.ThrowIfZero(points);
        ArgumentOutOfRangeException.ThrowIfZero(timePlayed);

        lock (_lock)
        {
            var jobStatisticsData = GetJobStatisticsData(jobId);
            jobStatisticsData.Points += points;
            jobStatisticsData.TimePlayed += timePlayed;
            VersionIncreased?.Invoke();
            PointsAdded?.Invoke(this, jobId, jobStatisticsData.Points);
            TimePlayedAdded?.Invoke(this, jobId, jobStatisticsData.TimePlayed);
        }
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
