namespace Realm.Domain.Components.Elements;

public class PedElementComponent : ElementComponent
{
    protected readonly Ped _ped;

    internal Ped Ped => _ped;

    internal override Element Element => _ped;

    internal PedElementComponent(Ped ped)
    {
        _ped = ped;
    }
}
