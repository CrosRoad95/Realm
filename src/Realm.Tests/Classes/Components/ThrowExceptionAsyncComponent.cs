using Realm.Domain.Components;

namespace Realm.Tests.Classes.Components;

internal class ThrowExceptionAsyncComponent : AsyncComponent
{
    protected override Task LoadAsync()
    {
        throw new Exception("Something went wrong");
    }
}
