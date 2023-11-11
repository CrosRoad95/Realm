namespace RealmCore.Server.Factories;

internal class DebounceFactory : IDebounceFactory
{
    public IDebounce Create(int milliseconds)
    {
        return new Debounce(milliseconds);
    }
}
