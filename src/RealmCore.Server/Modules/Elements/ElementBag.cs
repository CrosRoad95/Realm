namespace RealmCore.Server.Modules.Elements;

public class ElementBag : IEnumerable<Element>, IDisposable
{
    private bool _disposed;
    private readonly object _lock = new();
    private readonly List<Element> _elements = [];

    public bool TryAdd(Element element)
    {
        lock (_lock)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(ElementBag));

            if (element.IsDestroyed)
                return false;

            if (_elements.Contains(element))
                return false;

            _elements.Add(element);
            element.Destroyed += HandleDestroyed;

            if (element.IsDestroyed)
            {
                if (_elements.Remove(element))
                {
                    element.Destroyed -= HandleDestroyed;
                }
                return false;
            }
        }
        return true;
    }

    public bool TryRemove(Element element)
    {
        lock (_lock)
        {
            if (element.IsDestroyed)
                return false;

            if (_elements.Remove(element))
            {
                element.Destroyed -= HandleDestroyed;
                return true;
            }
        }
        return false;
    }

    private void HandleDestroyed(Element element)
    {
        lock (_lock)
        {
            _elements.Remove(element);
            element.Destroyed -= HandleDestroyed;
        }
    }

    public IEnumerator<Element> GetEnumerator()
    {
        Element[] view;
        lock (_lock)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(ElementBag));

            view = [.. _elements];
        }

        foreach (var element in view)
            yield return element;
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public void Dispose()
    {
        lock (_lock)
        {
            foreach (var element in _elements)
            {
                element.Destroyed -= HandleDestroyed;
            }
            _elements.Clear();

            _disposed = true;
        }
    }
}
