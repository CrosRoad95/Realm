namespace Realm.Server;

public class RPGPlayer : Player, IRPGPlayer
{
    public RPGPlayer()
    {

    }

    public void Spawn(ISpawn spawn)
    {
        Camera.Target = this;
        Camera.Fade(CameraFade.In);
        Spawn(spawn.Position, spawn.Rotation.Z, 0, 0, 0);
    }

    public override string ToString() => "Player";
}
