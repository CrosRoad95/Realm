using Realm.Server.Components;

namespace Realm.Tests.Classes.Components;

public class ThrowExceptionComponent : Component
{
    protected override void Load()
    {
        throw new Exception("Something went wrong");
    }
}
