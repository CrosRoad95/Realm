namespace Realm.Tests;

public class TestRPGPlayer : TestingPlayer, IRPGPlayer
{
    public CancellationToken CancellationToken { get; private set; }

    public new string Id => Name;

    public bool IsPersistant => true;

    private readonly CancellationTokenSource _cancellationTokenSource;

    public event Action<IRPGPlayer, int>? ResourceReady;

    public TestRPGPlayer()
    {
        _cancellationTokenSource = new CancellationTokenSource();
        CancellationToken = _cancellationTokenSource.Token;
    }

    public void Spawn(ISpawn spawn)
    {
        Camera.Target = this;
        Camera.Fade(CameraFade.In);
        Spawn(spawn.Position, spawn.Rotation.Z, 0, 0, 0);
    }

    public void TriggerClientEvent(string @event, object value)
    {

    }
}
