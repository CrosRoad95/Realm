namespace RealmCore.Server.Services.Players;

public interface IPlayerDailyVisitsService : IPlayerService
{
    DateTime LastVisit { get; set; }
    int VisitsInRow { get; set; }
    int VisitsInRowRecord { get; set; }

    event Action<IPlayerDailyVisitsService, int, bool>? Visited;
    event Action<IPlayerDailyVisitsService, int>? VisitsRecord;

    internal void Update(DateTime now);
}

internal sealed class PlayerDailyVisitsService : IPlayerDailyVisitsService, IDisposable
{
    private readonly object _lock = new();
    private readonly IPlayerUserService _playerUserService;
    private DailyVisitsData? _dailyVisitsData;
    public DateTime LastVisit
    {
        get => _dailyVisitsData?.LastVisit ?? throw new InvalidOperationException(); set
        {
            if (_dailyVisitsData == null)
                throw new InvalidOperationException();
            _dailyVisitsData.LastVisit = value;
        }
    }
    public int VisitsInRow
    {
        get => _dailyVisitsData?.VisitsInRow ?? throw new InvalidOperationException(); set
        {
            if (_dailyVisitsData == null)
                throw new InvalidOperationException();
            _dailyVisitsData.VisitsInRow = value;
        }
    }
    public int VisitsInRowRecord
    {
        get => _dailyVisitsData?.VisitsInRowRecord ?? throw new InvalidOperationException(); set
        {
            if (_dailyVisitsData == null)
                throw new InvalidOperationException();
            _dailyVisitsData.VisitsInRowRecord = value;
        }
    }

    public event Action<IPlayerDailyVisitsService, int, bool>? Visited;
    public event Action<IPlayerDailyVisitsService, int>? VisitsRecord;

    public RealmPlayer Player { get; init; }
    public PlayerDailyVisitsService(PlayerContext playerContext, IPlayerUserService playerUserService)
    {
        Player = playerContext.Player;
        playerUserService.SignedIn += HandleSignedIn;
        playerUserService.SignedOut += HandleSignedOut;
        _playerUserService = playerUserService;
    }

    private void HandleSignedIn(IPlayerUserService playerUserService, RealmPlayer player)
    {
        var now = player.GetRequiredService<IDateTimeProvider>().Now;
        lock (_lock)
        {
            if(playerUserService.User?.DailyVisits != null)
            {
                _dailyVisitsData = playerUserService.User?.DailyVisits;
            }
            else
            {
                _dailyVisitsData = new DailyVisitsData
                {
                    LastVisit = now,
                    VisitsInRow = 0,
                    VisitsInRowRecord = 0,
                };
            }
        }
        Update(now);
    }

    private void HandleSignedOut(IPlayerUserService playerUserService, RealmPlayer _)
    {
        lock (_lock)
            _dailyVisitsData = null;
    }

    public void Update(DateTime now)
    {
        var nowDate = now.Date;
        if (LastVisit.Date == nowDate)
            return;

        bool reset = false;

        if (LastVisit.Date.AddDays(1) == nowDate || LastVisit == DateTime.MinValue)
        {
            VisitsInRow++;
        }
        else
        {
            VisitsInRow = 0;
            reset = true;
        }

        if (VisitsInRow > VisitsInRowRecord) // Doesn't check if day passed because value can be arbitrarily changed
        {
            VisitsInRowRecord = VisitsInRow;
            VisitsRecord?.Invoke(this, VisitsInRowRecord);
        }

        _playerUserService.IncreaseVersion();
        Visited?.Invoke(this, VisitsInRow, reset);
        LastVisit = nowDate;
    }

    public void Dispose()
    {

    }
}
