namespace Realm.Scripting.Classes;

public class EventFunctions
{
    private readonly HashSet<string> _supportedEventsNames = new();
    private readonly Dictionary<string, List<ScriptObject>> _events = new();

    [NoScriptAccess]
    public async Task InvokeEvent(string eventName, params object[] arguments)
    {
        if (!_supportedEventsNames.Contains(eventName))
            throw new NotImplementedException();

        if(_events.TryGetValue(eventName, out var events))
        {
            foreach (var scriptObject in events)
            {
                if (scriptObject.IsAsync())
                    await (scriptObject.Invoke(false, arguments) as dynamic);
                else
                    scriptObject.Invoke(false, arguments);
            }
        }
    }

    [NoScriptAccess]
    public void RegisterEvent(string eventName)
    {
        _supportedEventsNames.Add(eventName);
    }

    public bool AddEventHandler(string eventName, ScriptObject callback)
    {
        if (!_supportedEventsNames.Contains(eventName))
            return false;

        if (!_events.ContainsKey(eventName))
            _events[eventName] = new List<ScriptObject>();
        _events[eventName].Add(callback);
        return true;
    }

    public bool RemoveEventHandler(string eventName, ScriptObject callback)
    {
        if (!_supportedEventsNames.Contains(eventName) || !_events.ContainsKey(eventName))
            return false;

        int removed = _events[eventName].RemoveAll(x => x.Is(callback));
        if (!_events[eventName].Any())
            _events.Remove(eventName);
        return true;
    }

    public override string ToString() => "Event";
}
