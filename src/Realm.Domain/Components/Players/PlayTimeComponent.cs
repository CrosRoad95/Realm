namespace Realm.Domain.Components.Players;

[ComponentUsage(false)]
public class PlayTimeComponent : Component
{
    private DateTime? _startDateTime;

    public ulong PlayTime
    {
        get
        {
            if (_startDateTime == null)
                return 0;
            return (ulong)(DateTime.Now - _startDateTime.Value).Seconds;
        }
    }

    public ulong TotalPlayTime
    {
        get
        {
            ulong totalPlayTime = PlayTime;
            if(Entity.TryGetComponent(out AccountComponent accountComponent))
            {
                totalPlayTime += accountComponent.User.PlayTime;
            }
            return totalPlayTime;
        }
    }

    public PlayTimeComponent()
    {
        _startDateTime = DateTime.Now;
    }

    public void Reset()
    {
        _startDateTime = DateTime.Now;
    }
}
