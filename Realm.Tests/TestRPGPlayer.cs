using Realm.Interfaces.Scripting.Classes;

namespace Realm.Tests;

public class TestRPGPlayer : TestingPlayer, IRPGPlayer
{
    public void Spawn(ISpawn spawn)
    {
        Camera.Target = this;
        Camera.Fade(CameraFade.In);
        Spawn(spawn.Position, spawn.Rotation.Z, 0, 0, 0);
    }
}
