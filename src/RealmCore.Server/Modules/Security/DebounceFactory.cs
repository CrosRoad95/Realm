namespace RealmCore.Server.Modules.Security;

public interface IDebounceFactory
{
    IDebounce Create(int milliseconds);
}

internal sealed class DebounceFactory : IDebounceFactory
{
    public IDebounce Create(int milliseconds)
    {
        return new Debounce(milliseconds);
    }
}
