namespace Realm.Scripting.Classes;

public class Event : IEvent
{
    private readonly IPlayerManager _playerManager;

    private readonly List<ScriptObject> _playerJoinedEvents = new();
    public Event(IPlayerManager playerManager)
    {
        _playerManager = playerManager;
        _playerManager.PlayerJoined += PlayerManager_PlayerJoined;
    }

    private void PlayerManager_PlayerJoined(IRPGPlayer player)
    {
        var @event = new PlayerJoinedEvent
        {
            Player = player,
        };

        foreach (var scriptObject in _playerJoinedEvents)
        {
            scriptObject.Invoke(false, @event);
        }
    }

    public bool AddHandler(string eventName, ScriptObject callback)
    {
        switch(eventName)
        {
            case "onPlayerJoin":
                _playerJoinedEvents.Add(callback);
                return true;
        }
        return false;
    }

    //public bool RemoveHandler(string eventName, ScriptObject callback)
    //{
    //    switch(eventName)
    //    {
    //        case "onPlayerJoin":
    //            var a = _playerJoinedEvents.Remove(callback.);
    //            return _playerJoinedEvents.Remove(callback);
    //    }
    //    return false;
    //}

    public override string ToString() => "Event";
}
