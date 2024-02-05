namespace RealmCore.Server.Services.Players;

public interface IPlayerJobStatisticsService : IPlayerService, IEnumerable<UserJobStatisticsDTO>
{
    void AddPoints(short jobId, ulong points);
    void AddPointsAndTimePlayed(short jobId, ulong points, ulong timePlayed);
    void AddTimePlayed(short jobId, ulong timePlayed);
    (ulong, ulong) GetTotalPoints(short jobId);
}

internal class PlayerJobStatisticsService : IPlayerJobStatisticsService
{
    private ICollection<JobStatisticsData> _jobStatistics = [];
    private readonly object _lock = new();
    private readonly IPlayerUserService _playerUserService;
    private readonly IDateTimeProvider _dateTimeProvider;

    public event Action<IPlayerJobStatisticsService, short, ulong>? PointsAdded;
    public event Action<IPlayerJobStatisticsService, short, ulong>? TimePlayedAdded;

    public RealmPlayer Player { get; private set; }
    public PlayerJobStatisticsService(PlayerContext playerContext, IPlayerUserService playerUserService, IDateTimeProvider dateTimeProvider)
    {
        Player = playerContext.Player;
        playerUserService.SignedIn += HandleSignedIn;
        playerUserService.SignedOut += HandleSignedOut;
        _playerUserService = playerUserService;
        _dateTimeProvider = dateTimeProvider;
    }

    private void HandleSignedIn(IPlayerUserService playerUserService, RealmPlayer _)
    {
        lock (_lock)
            _jobStatistics = playerUserService.User.JobStatistics;
    }

    private void HandleSignedOut(IPlayerUserService playerUserService, RealmPlayer _)
    {
        lock (_lock)
            _jobStatistics = [];
    }

    private JobStatisticsData GetJobStatisticsData(short jobId)
    {
        var today = DateOnly.FromDateTime(_dateTimeProvider.Now);
        var jobStatisticsData = _jobStatistics.Where(x => x.JobId == jobId && x.Date == today).FirstOrDefault();
        if(jobStatisticsData == null)
        {
            jobStatisticsData = new JobStatisticsData
            {
                JobId = jobId,
                Date = today,
            };
            _playerUserService.IncreaseVersion();
            _jobStatistics.Add(jobStatisticsData);
        }
        return jobStatisticsData;
    }

    public void AddPoints(short jobId, ulong points)
    {
        ArgumentOutOfRangeException.ThrowIfZero(points);

        lock (_lock)
        {
            var jobStatisticsData = GetJobStatisticsData(jobId);
            jobStatisticsData.Points += points;
            _playerUserService.IncreaseVersion();
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
            _playerUserService.IncreaseVersion();
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
            _playerUserService.IncreaseVersion();
            PointsAdded?.Invoke(this, jobId, jobStatisticsData.Points);
            TimePlayedAdded?.Invoke(this, jobId, jobStatisticsData.TimePlayed);
        }
    }

    public (ulong, ulong) GetTotalPoints(short jobId)
    {
        lock (_lock)
        {
            ulong totalPoints = 0;
            ulong totalTimePlayed = 0;
            foreach (var jobStatistic in _jobStatistics)
            {
                if(jobStatistic.JobId == jobId)
                {
                    totalPoints += jobStatistic.Points;
                    totalTimePlayed += jobStatistic.TimePlayed;
                }
            }
            return (totalPoints, totalTimePlayed);
        }
    }

    [return: NotNullIfNotNull(nameof(userJobStatisticsData))]
    private static UserJobStatisticsDTO? Map(JobStatisticsData? userJobStatisticsData)
    {
        if (userJobStatisticsData == null)
            return null;

        return new UserJobStatisticsDTO
        {
            JobId = userJobStatisticsData.JobId,
            Points = userJobStatisticsData.Points,
            TimePlayed = userJobStatisticsData.TimePlayed,
            UserId = userJobStatisticsData.UserId
        };
    }

    public IEnumerator<UserJobStatisticsDTO> GetEnumerator()
    {
        lock (_lock)
            return _jobStatistics.Select(Map).ToList().GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
