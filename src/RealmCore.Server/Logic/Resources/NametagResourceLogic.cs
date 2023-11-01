using RealmCore.Server.Components.Peds;

namespace RealmCore.Server.Logic.Resources;

internal sealed class NametagResourceLogic : ComponentLogic<NametagComponent>
{
    private readonly INametagsService _nametagsService;

    public NametagResourceLogic(IElementFactory elementFactory, INametagsService nametagsService) : base(elementFactory)
    {
        _nametagsService = nametagsService;
    }

    protected override void ComponentAdded(NametagComponent nametagComponent)
    {
        _nametagsService.SetNametag((Ped)nametagComponent.Element, nametagComponent.Text);
        nametagComponent.TextChanged += HandleTextChanged;
    }

    protected override void ComponentDetached(NametagComponent nametagComponent)
    {
        _nametagsService.RemoveNametag((Ped)nametagComponent.Element);
    }

    private void HandleTextChanged(NametagComponent nametagComponent, string text)
    {
        _nametagsService.SetNametag((Ped)nametagComponent.Element, text);
    }

}
