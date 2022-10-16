using Realm.Interfaces.Discord;
using Realm.Interfaces.Scripting.Classes;
using Realm.Scripting.Classes.Contextes;
using Realm.Scripting.Extensions;

namespace Realm.Scripting.Classes;

public class Event : IEvent
{
    private readonly IPlayerManager _playerManager;
    private readonly List<ScriptObject> _playerJoinedEvents = new();
    private readonly List<ScriptObject> _discordStatusChannelUpdate = new();
    public Event(IPlayerManager playerManager, IStatusChannel statusChannel)
    {
        _playerManager = playerManager;
        _playerManager.PlayerJoined += PlayerManager_PlayerJoined;
        statusChannel.BeginStatusChannelUpdate = BeginStatusChannelUpdate;
    }

    private async Task<string> BeginStatusChannelUpdate()
    {
        var discordStatusChannelUpdateContext = new DiscordStatusChannelUpdateContext();
        foreach (var scriptObject in _discordStatusChannelUpdate)
        {
            if (scriptObject.IsAsync())
                await scriptObject.Invoke(false, discordStatusChannelUpdateContext).ToTask();
            else
                scriptObject.Invoke(false, discordStatusChannelUpdateContext);
        }

        return discordStatusChannelUpdateContext.Content;
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

    private List<ScriptObject>? GetSubscribersGroupByEventName(string eventName)
    {
        return eventName switch
        {
            "onPlayerJoin" => _playerJoinedEvents,
            "onDiscordStatusChannelUpdate" => _discordStatusChannelUpdate,
            _ => null,
        };
    }

    public bool AddHandler(string eventName, ScriptObject callback)
    {
        var group = GetSubscribersGroupByEventName(eventName);
        if (group == null)
            return false;
        group.Add(callback);
        return true;
    }

    public bool RemoveHandler(string eventName, ScriptObject callback)
    {
        var group = GetSubscribersGroupByEventName(eventName);
        if (group == null)
            return false;
        group.Remove(callback);
        return true;
    }

    public void Reload()
    {
        _playerJoinedEvents.Clear();
    }

    public int GetPriority() => 10000;

    public override string ToString() => "Event";
}
