
using RealmCore.Server.Concepts.Interactions;

namespace RealmCore.Server.Interfaces;

public interface IInteraction
{
    Interaction? Interaction { get; set; }
    event Action<Element, Interaction?, Interaction?>? InteractionChanged;
}
