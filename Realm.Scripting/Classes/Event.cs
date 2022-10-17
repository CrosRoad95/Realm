using Realm.Interfaces;
using Realm.Scripting.Classes.Contextes;

namespace Realm.Scripting.Classes;

public class Event : IEvent
{
    private readonly IPlayerManager _playerManager;
    private readonly IRPGServer _server;
    private readonly List<ScriptObject> _playerJoinedEvents = new();
    private readonly List<ScriptObject> _discordStatusChannelUpdate = new();
    private readonly List<ScriptObject> _formSubmitEvents = new();
    public Event(IPlayerManager playerManager, IStatusChannel statusChannel, IRPGServer server)
    {
        _playerManager = playerManager;
        _server = server;
        _playerManager.PlayerJoined += PlayerManager_PlayerJoined;
        statusChannel.BeginStatusChannelUpdate = BeginStatusChannelUpdate;
        _server.SubscribeLuaEvent("internalSubmitForm", HandleForSubmissions);
    }

    public async Task HandleForSubmissions(ILuaEventContext context)
    {
        var name = context.GetValue<string>(1) as string;
        var data = ConvertDictionary<string, string>(context.GetValue<Dictionary<string, string>>(2));

        var formSubmitDataEvent = new FormSubmitDataEvent
        {
            Player = context.Player,
            Name = name,
            Data = data,
        };
        foreach (var scriptObject in _formSubmitEvents)
        {
            if (scriptObject.IsAsync())
                await scriptObject.Invoke(false, formSubmitDataEvent).ToTask();
            else
                scriptObject.Invoke(false, formSubmitDataEvent);
        }

        if(formSubmitDataEvent.Errors.Any())
            context.Response(name, false, formSubmitDataEvent.Errors);
        else
            context.Response(name, true);
    }

    private Dictionary<TKey, TValue> ConvertDictionary<TKey, TValue>(object? obj)
        where TKey : class
        where TValue : class
    {
        return (obj as Dictionary<object, object>)
            .ToDictionary(kv => kv.Key as TKey, kv => kv.Value as TValue);
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
            "onFormSubmit" => _formSubmitEvents,
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
        group.RemoveAll(x => x.Is(callback));
        return true;
    }

    public void Reload()
    {
        _playerJoinedEvents.Clear();
    }

    public int GetPriority() => 10000;

    public override string ToString() => "Event";
}
