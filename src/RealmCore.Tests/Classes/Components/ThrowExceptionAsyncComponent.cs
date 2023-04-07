using RealmCore.Server.Components;

namespace RealmCore.Tests.Classes.Components;

internal class ThrowExceptionAsyncComponent : AsyncComponent
{
    protected override Task LoadAsync()
    {
        throw new Exception("Something went wrong");
    }
}
