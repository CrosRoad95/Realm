namespace Realm.Tests;

public class TestRPGPlayer : TestingPlayer, IRPGPlayer
{
    public void Spawn(Vector3 position, Vector3 rotation)
    {
        Camera.Target = this;
        Camera.Fade(CameraFade.In);
        Spawn(position, rotation.Z, 0, 0, 0);
    }
}
