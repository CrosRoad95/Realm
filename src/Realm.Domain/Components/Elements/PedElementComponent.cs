namespace Realm.Domain.Components.Elements;

public class PedElementComponent : ElementComponent
{
    protected readonly Ped _pickup;

    internal override Element Element => _pickup;

    internal PedElementComponent(Ped ped)
    {
        _pickup = ped;
    }
}
