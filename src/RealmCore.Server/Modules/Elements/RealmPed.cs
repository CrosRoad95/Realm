namespace RealmCore.Server.Modules.Elements;

public class RealmPed : Ped
{
    public IElementCustomDataFeature CustomData { get; init; } = new ElementCustomDataFeature();

    public Nametag Nametag { get; init; }

    public RealmPed(PedModel model, Vector3 position) : base(model, position)
    {
        Nametag = new(this);
    }
}
