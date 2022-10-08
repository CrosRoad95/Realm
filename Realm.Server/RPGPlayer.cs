namespace Realm.Server;

public class RPGPlayer : Player, IRPGPlayer
{
    public RPGPlayer()
    {

    }

    public override string ToString() => Name;
}
