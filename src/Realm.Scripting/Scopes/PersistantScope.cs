namespace Realm.Module.Scripting.Scopes;

public class PersistantScope : IDisposable
{
    public static bool IsPersistant { get; set; }

    public PersistantScope()
    {
        if (IsPersistant)
            throw new Exception("Failed to enter scope, already in it.");
        IsPersistant = true;
    }

    public void Dispose()
    {
        IsPersistant = false;
    }
}
