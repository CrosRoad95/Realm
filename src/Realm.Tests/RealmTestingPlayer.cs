namespace Realm.Tests;

public class RealmTestingPlayer : TestingPlayer
{
    public CancellationToken CancellationToken { get; private set; }

    public new string Id => Name;

    private readonly CancellationTokenSource _cancellationTokenSource;

    public RealmTestingPlayer()
    {
        _cancellationTokenSource = new CancellationTokenSource();
        CancellationToken = _cancellationTokenSource.Token;
    }

    //public void Spawn(ISpawn spawn)
    //{
    //    Camera.Target = this;
    //    Camera.Fade(CameraFade.In);
    //    Spawn(spawn.Position, spawn.Rotation.Z, 0, 0, 0);
    //}

    public void TriggerClientEvent(string @event, params object[] values)
    {

    }
}
