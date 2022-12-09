namespace Realm.Scripting.Functions;

[NoDefaultScriptAccess]
public class EventScriptingFunctions : IReloadable
{
    private readonly HashSet<string> _supportedEventsNames = new();
    private readonly Dictionary<string, List<ScriptObject>> _events = new();
    private readonly ILogger _logger;

    public EventScriptingFunctions(ILogger logger)
    {
        _logger = logger.ForContext<EventScriptingFunctions>();
    }

    public async Task InvokeEvent(string eventName, params object[] arguments)
    {
        if (!_supportedEventsNames.Contains(eventName))
            throw new NotImplementedException();

        if (_events.TryGetValue(eventName, out var events))
        {
            using var _ = LogContext.PushProperty("eventName", eventName);
            foreach (var scriptObject in events)
            {
                try
                {
                    if (scriptObject.IsAsync())
                        await (scriptObject.Invoke(false, arguments) as dynamic);
                    else
                        scriptObject.Invoke(false, arguments);
                }
                catch (ScriptEngineException scriptEngineException)
                {
                    var scriptException = scriptEngineException as IScriptEngineException;
                    if (scriptException != null)
                    {
                        using var errorDetails = LogContext.PushProperty("errorDetails", scriptException.ErrorDetails);
                        _logger.Error(scriptEngineException, "Exception thrown while executing event");
                    }
                    else
                        _logger.Error(scriptEngineException, "Exception thrown while executing event");
                }
            }
        }
    }

    public void RegisterEvent(string eventName)
    {
        _supportedEventsNames.Add(eventName);
    }

    [ScriptMember("addEventHandler")]
    public bool AddEventHandler(string eventName, ScriptObject callback)
    {
        if (!_supportedEventsNames.Contains(eventName))
            return false;

        if (!_events.ContainsKey(eventName))
            _events[eventName] = new List<ScriptObject>();
        _events[eventName].Add(callback);
        return true;
    }

    [ScriptMember("removeEventHandler")]
    public bool RemoveEventHandler(string eventName, ScriptObject callback)
    {
        if (!_supportedEventsNames.Contains(eventName) || !_events.ContainsKey(eventName))
            return false;

        int removed = _events[eventName].RemoveAll(x => x.Is(callback));
        if (!_events[eventName].Any())
            _events.Remove(eventName);
        return true;
    }

    [ScriptMember("toString")]
    public override string ToString() => "Event";

    public Task Reload()
    {
        _events.Clear();
        return Task.CompletedTask;
    }

    public int GetPriority() => 60;
}
