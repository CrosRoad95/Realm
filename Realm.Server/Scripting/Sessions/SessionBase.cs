using System.Diagnostics;

namespace Realm.Server.Scripting.Sessions;

public abstract class SessionBase
{
    private readonly DateTime _startTime = DateTime.Now;
    private readonly Stopwatch _stopwatch = new Stopwatch();
    private readonly string _sessionId = Guid.NewGuid().ToString();
    [ScriptMember("sessionId")]
    public string SessionId { [ScriptUsage()] get => _sessionId; }

    public TimeSpan Elapsed => _stopwatch.Elapsed;

    public void Start()
    {
        _stopwatch.Start();
    }

    public void Stop()
    {
        _stopwatch.Stop();
    }
}
