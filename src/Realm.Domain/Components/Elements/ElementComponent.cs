using Realm.Domain.Interfaces;

namespace Realm.Domain.Components.Elements;

public abstract class ElementComponent : Component
{
    [Inject]
    protected IEntityByElement EntityByElement { get; set; } = default!;

    abstract public Element Element { get; }
}
