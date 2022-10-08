namespace Realm.Scripting.Classes;

internal class Player : IPlayer
{
    private readonly IRPGPlayer _rpgPlayer;
    private readonly Guid _id;

    public Player(IRPGPlayer rpgPlayer, Guid id)
    {
        _rpgPlayer = rpgPlayer;
        _id = id;
    }

    public string Id => _id.ToString();
}
