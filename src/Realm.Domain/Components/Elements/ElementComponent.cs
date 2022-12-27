using Realm.Domain.Interfaces;

namespace Realm.Domain.Components.Elements;

public abstract class ElementComponent : Component
{
    private IEntityByElement? _elementById;
    protected IEntityByElement ElementById => _elementById ?? throw new Exception();
    abstract public Element Element { get; }

    public override Task Load()
    {
        _elementById = Entity.GetRequiredService<IEntityByElement>();
        return Task.CompletedTask;
    }
}
