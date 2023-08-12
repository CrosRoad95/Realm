using RealmCore.Server.Components.Peds;

namespace RealmCore.Server.Logic.Resources;

internal sealed class NametagResourceLogic : ComponentLogic<NametagComponent>
{
    private readonly INametagsService _nametagsService;

    public NametagResourceLogic(IECS ecs, INametagsService nametagsService) : base(ecs)
    {
        _nametagsService = nametagsService;
    }

    protected override void ComponentAdded(NametagComponent nametagComponent)
    {
        _nametagsService.SetNametag((Ped)nametagComponent.Entity.Element, nametagComponent.Text);
        nametagComponent.TextChanged += HandleTextChanged;
    }

    protected override void ComponentRemoved(NametagComponent nametagComponent)
    {
        _nametagsService.RemoveNametag((Ped)nametagComponent.Entity.Element);
    }

    private void HandleTextChanged(NametagComponent nametagComponent, string text)
    {
        _nametagsService.SetNametag((Ped)nametagComponent.Entity.Element, text);
    }

}
