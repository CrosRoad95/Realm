namespace RealmCore.Server.Modules.Players.DailyVisits;

public interface IPlayerDailyTasksFeature : IPlayerFeature
{
    DailyTaskProgressDto[] Today { get; }

    DailyTaskProgressDto? GetTask(int taskId, DateTime? at);
    float GetProgress(int taskId, DateTime? at = null);
    bool TryBeginDailyTask(int taskId);
    bool TryDoProgress(int taskId, float progress, DateTime? at = null);
    DailyTaskProgressDto[] GetTasks(DateTime? at = null);
}

public sealed class DailyTaskProgressDto : IEquatable<DailyTaskProgressDto>
{
    public required int Id { get; init; }
    public required DateTime CreatedAt { get; init; }
    public required int DailyTaskId { get; init; }
    public required float Progress { get; init; }

    [return: NotNullIfNotNull(nameof(userDailyTaskProgressData))]
    public static DailyTaskProgressDto? Map(UserDailyTaskProgressData? userDailyTaskProgressData)
    {
        if (userDailyTaskProgressData == null)
            return null;

        return new DailyTaskProgressDto
        {
            Id = userDailyTaskProgressData.Id,
            CreatedAt = userDailyTaskProgressData.CreatedAt,
            DailyTaskId = userDailyTaskProgressData.DailyTaskId,
            Progress = userDailyTaskProgressData.Progress,
        };
    }

    public bool Equals(DailyTaskProgressDto? other)
    {
        if (other == null)
            return false;

        return other.Id == Id;
    }
}


internal sealed class PlayerDailyTasksFeature : IPlayerDailyTasksFeature, IUsesUserPersistentData
{
    private readonly object _lock = new();
    private readonly IDateTimeProvider _dateTimeProvider;
    private ICollection<UserDailyTaskProgressData> _dailyTasksProgressData = [];

    public DailyTaskProgressDto[] Today => GetTasks();

    public RealmPlayer Player { get; init; }
    public PlayerDailyTasksFeature(PlayerContext playerContext, IDateTimeProvider dateTimeProvider)
    {
        Player = playerContext.Player;
        _dateTimeProvider = dateTimeProvider;
    }

    public event Action? VersionIncreased;

    public void LogIn(UserData userData)
    {
        lock(_lock)
        _dailyTasksProgressData = userData.DailyTasksProgress;
    }

    public bool TryBeginDailyTask(int taskId)
    {
        var now = _dateTimeProvider.Now;
        lock (_lock)
        {
            lock (_lock)
            {
                if (_dailyTasksProgressData.Where(x => AreSameDay(x.CreatedAt, now) && x.DailyTaskId == taskId).Any())
                    return false;

                _dailyTasksProgressData.Add(new UserDailyTaskProgressData
                {
                    CreatedAt = now,
                    DailyTaskId = taskId
                });
            }

            return true;
        }
    }

    public DailyTaskProgressDto? GetTask(int taskId, DateTime? at = null)
    {
        at ??= _dateTimeProvider.Now;
        lock (_lock)
        {
            var data = _dailyTasksProgressData.Where(x => AreSameDay(x.CreatedAt, at.Value) && x.DailyTaskId == taskId).FirstOrDefault();
            return DailyTaskProgressDto.Map(data);
        }
    }

    public DailyTaskProgressDto[] GetTasks(DateTime? at = null)
    {
        at ??= _dateTimeProvider.Now;
        lock (_lock)
        {
            var data = _dailyTasksProgressData.Where(x => AreSameDay(x.CreatedAt, at.Value));
            return data.Select(DailyTaskProgressDto.Map).ToArray();
        }
    }

    public float GetProgress(int taskId, DateTime? at = null)
    {
        at ??= _dateTimeProvider.Now;
        lock (_lock)
        {
            var data = _dailyTasksProgressData.Where(x => AreSameDay(x.CreatedAt, at.Value) && x.DailyTaskId == taskId).FirstOrDefault();
            return data?.Progress ?? 0;
        }
    }

    public bool TryDoProgress(int taskId, float progress, DateTime? at = null)
    {
        at ??= _dateTimeProvider.Now;
        var now = _dateTimeProvider.Now;
        lock (_lock)
        {
            var data = _dailyTasksProgressData.Where(x => AreSameDay(at.Value, now) && x.DailyTaskId == taskId).FirstOrDefault();
            if(data == null)
                return false;

            data.Progress += progress;
            return true;
        }
    }

    private bool AreSameDay(DateTime dateTime1, DateTime dateTime2)
    {
        return dateTime1.Year == dateTime2.Year && dateTime1.Month == dateTime2.Month && dateTime1.Day == dateTime2.Day;
    }
}
