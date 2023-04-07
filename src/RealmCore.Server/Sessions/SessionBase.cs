using System.Diagnostics;

namespace RealmCore.Server.Sessions;

public abstract class SessionBase
{
    private readonly Stopwatch _stopwatch = new Stopwatch();
    private readonly string _sessionId = Guid.NewGuid().ToString();
    private readonly string _code;

    public string SessionId { get => _sessionId; }
    public string Code { get => _code; }
    public double Elapsed { get => _stopwatch.ElapsedMilliseconds; }

    public SessionBase(string code)
    {
        _code = code;
    }

    public void Start()
    {
        _stopwatch.Start();
    }

    public void Stop()
    {
        _stopwatch.Stop();
    }
}
