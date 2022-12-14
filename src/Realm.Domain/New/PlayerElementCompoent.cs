namespace Realm.Domain.New;

[NoDefaultScriptAccess]
public sealed class PlayerElementCompoent : Component
{
    private readonly RPGPlayer _rpgPlayer;

    public PlayerElementCompoent(RPGPlayer rpgPlayer)
    {
        _rpgPlayer = rpgPlayer;
    }
}
