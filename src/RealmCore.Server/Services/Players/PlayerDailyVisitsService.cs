namespace RealmCore.Server.Services.Players;

internal sealed class PlayerDailyVisitsService : IPlayerDailyVisitsService, IDisposable
{
    private readonly object _lock = new();
    private readonly IDateTimeProvider _dateTimeProvider;
    private DailyVisitsData? _dailyVisitsData;
    public DateTime LastVisit { get => _dailyVisitsData.LastVisit; set => _dailyVisitsData.LastVisit = value; }
    public int VisitsInRow { get => _dailyVisitsData.VisitsInRow; set => _dailyVisitsData.VisitsInRow = value; }
    public int VisitsInRowRecord { get => _dailyVisitsData.VisitsInRowRecord; set => _dailyVisitsData.VisitsInRowRecord = value; }

    public event Action<IPlayerDailyVisitsService, int, bool>? PlayerVisited;
    public event Action<IPlayerDailyVisitsService, int>? PlayerVisitsRecord;
    
    public RealmPlayer Player { get; }
    public PlayerDailyVisitsService(PlayerContext playerContext, IPlayerUserService playerUserService, IDateTimeProvider dateTimeProvider)
    {
        Player = playerContext.Player;
        playerUserService.SignedIn += HandleSignedIn;
        playerUserService.SignedOut += HandleSignedOut;
        _dateTimeProvider = dateTimeProvider;
    }

    private void HandleSignedIn(IPlayerUserService playerUserService)
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

    private void HandleSignedOut(IPlayerUserService playerUserService)
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
            PlayerVisitsRecord?.Invoke(this, VisitsInRowRecord);
        }

        PlayerVisited?.Invoke(this, VisitsInRow, reset);
        LastVisit = nowDate;
    }

    public void Dispose()
    {

    }
}
