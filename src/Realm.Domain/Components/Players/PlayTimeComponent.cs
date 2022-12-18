namespace Realm.Domain.Components.Players;

public class PlayTimeComponent
{
    private DateTime? _startDateTime;

    [ScriptMember("playTime")]
    public ulong PlayTime
    {
        get
        {
            if (_startDateTime == null)
                return 0;
            return (ulong)(DateTime.Now - _startDateTime.Value).Seconds;
        }
    }

    public PlayTimeComponent()
    {
        _startDateTime = DateTime.Now;
    }

    [ScriptMember("reset")]
    public void Reset()
    {
        _startDateTime = DateTime.Now;
    }
}
