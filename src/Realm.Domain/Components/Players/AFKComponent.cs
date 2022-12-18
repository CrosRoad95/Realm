using Realm.Resources.AFK;

namespace Realm.Domain.Components.Players;

public class AFKComponent : Component
{
    public AFKComponent()
    {

    }

    public override Task Load()
    {
        var afkService = Entity.GetRequiredService<AFKService>();
        return Task.CompletedTask;
    }
}
