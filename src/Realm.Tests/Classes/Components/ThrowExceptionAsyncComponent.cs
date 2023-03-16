namespace Realm.Tests.Classes.Components;

internal class ThrowExceptionAsyncComponent : Component
{
    protected override Task LoadAsync()
    {
        throw new Exception("Something went wrong");
    }
}
