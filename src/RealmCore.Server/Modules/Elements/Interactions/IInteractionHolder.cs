namespace RealmCore.Server.Modules.Elements.Interactions;

public interface IInteractionHolder
{
    Interaction? Interaction { get; set; }
    event Action<Element, Interaction?, Interaction?>? InteractionChanged;
}
