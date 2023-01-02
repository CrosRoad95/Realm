using Realm.Resources.AFK;

namespace Realm.Domain.Components.Players;

public class AFKComponent : Component
{
    [Inject]
    private AFKService AFKService { get; set; } = default!;

    public AFKComponent()
    {

    }
}
