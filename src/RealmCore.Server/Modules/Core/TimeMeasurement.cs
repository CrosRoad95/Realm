namespace RealmCore.Server.Modules.Core;

public class TimeMeasurement
{
    private IDateTimeProvider _dateTimeProvider;

    private DateTime _startTime;
    private AtomicBool _isRunning;
    private TimeSpan _elapsed;

    public bool IsRunning => _isRunning;

    public TimeMeasurement(IDateTimeProvider dateTimeProvider)
    {
        _dateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
    }

    public bool TryStart()
    {
        if (_isRunning.TrySetTrue())
        {
            _startTime = _dateTimeProvider.Now;
            return true;
        }
        return false;
    }

    public bool TryStop()
    {
        if (_isRunning.TrySetFalse())
        {
            _elapsed += _dateTimeProvider.Now - _startTime;
            return true;
        }
        return false;
    }

    public TimeSpan Elapsed => _isRunning ? _elapsed + (_dateTimeProvider.Now - _startTime) : _elapsed;
}
