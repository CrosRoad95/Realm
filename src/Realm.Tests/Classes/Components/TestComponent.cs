using Microsoft.AspNetCore.Components;
using Realm.Server.Components;

namespace Realm.Tests.Classes.Components;

public class TestComponent : Component
{
    [Inject]
    private object? Object { get; set; }
    public bool IsDispoed()
    {
        try
        {
            ThrowIfDisposed();
            return false;
        }
        catch
        {
            return true;
        }
    }

    public bool isObjectDefined() => Object != null;
}
