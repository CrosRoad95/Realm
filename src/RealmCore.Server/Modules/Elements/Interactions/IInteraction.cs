namespace RealmCore.Server.Modules.Elements.Interactions;

public interface IInteraction
{
    Interaction? Interaction { get; set; }
    event Action<Element, Interaction?, Interaction?>? InteractionChanged;
}
