namespace Realm.Domain.Components.Players;

[ComponentUsage(false)]
public class PlayTimeComponent : Component
{
    private DateTime? _startDateTime;
    private readonly ulong _currentPlayTime = 0;

    public ulong PlayTime
    {
        get
        {
            if (_startDateTime == null)
                return 0;
            return (ulong)(DateTime.Now - _startDateTime.Value).Seconds;
        }
    }

    public ulong TotalPlayTime => PlayTime + _currentPlayTime;

    public PlayTimeComponent()
    {
        _startDateTime = DateTime.Now;
    }

    public PlayTimeComponent(ulong currentPlayTime)
    {
        _currentPlayTime = currentPlayTime;
        _startDateTime = DateTime.Now;
    }

    public void Reset()
    {
        _startDateTime = DateTime.Now;
    }
}
