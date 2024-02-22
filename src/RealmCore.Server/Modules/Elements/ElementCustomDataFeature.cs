namespace RealmCore.Server.Modules.Elements;

public interface IElementCustomDataFeature : IElementFeature
{
    T? Get<T>() where T : class;
    void Set<T>(T value) where T : class;
}

internal class ElementCustomDataFeature : IElementCustomDataFeature
{
    private object? _customData = null;

    public void Set<T>(T value) where T : class
    {
        _customData = value;
    }

    public T? Get<T>() where T : class
    {
        return (T?)_customData;
    }

    public bool TryGet<T>(out T value) where T : class
    {
        if(_customData is T customData)
        {
            value = customData;
            return true;
        }
        value = null!;
        return false;
    }
}
