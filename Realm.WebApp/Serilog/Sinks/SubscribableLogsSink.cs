using Serilog.Core;
using Serilog.Events;

namespace Realm.WebApp.Serilog.Sinks;

public class SubscribableLogsSink : ILogEventSink
{
    public event Action<LogEvent>? LogEmited;
    public void Emit(LogEvent logEvent)
    {
        LogEmited?.Invoke(logEvent);
    }
}
