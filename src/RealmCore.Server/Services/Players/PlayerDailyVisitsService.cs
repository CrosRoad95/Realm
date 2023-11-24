namespace RealmCore.Server.Services.Players;

internal sealed class PlayerDailyVisitsService : IPlayerDailyVisitsService, IDisposable
{
    private readonly object _lock = new();
    private readonly IPlayerUserService _playerUserService;
    private readonly IDateTimeProvider _dateTimeProvider;
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
    
    public RealmPlayer Player { get; }
    public PlayerDailyVisitsService(PlayerContext playerContext, IPlayerUserService playerUserService, IDateTimeProvider dateTimeProvider)
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
        {
            if(playerUserService.User?.DailyVisits != null)
            {
                _dailyVisitsData = playerUserService.User?.DailyVisits;
            }
            else
            {
                _dailyVisitsData = new DailyVisitsData
                {
                    LastVisit = _dateTimeProvider.Now,
                    VisitsInRow = 0,
                    VisitsInRowRecord = 0,
                };
            }
        }
        Update(_dateTimeProvider.Now);
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
