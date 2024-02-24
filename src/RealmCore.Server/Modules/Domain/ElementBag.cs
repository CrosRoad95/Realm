namespace RealmCore.Server.Modules.Domain;

public class ElementBag : IEnumerable<Element>, IDisposable
{
    private readonly object _lock = new();
    private readonly List<Element> _elements = [];

    public bool Add(Element element)
    {
        if (element.IsDestroyed)
            throw new ElementDestroyedException(element);

        lock (_lock)
        {
            if (_elements.Contains(element))
                return false;

            _elements.Add(element);
            element.Destroyed += HandleDestroyed;
            if (element.IsDestroyed)
            {
                _elements.Remove(element);
                element.Destroyed -= HandleDestroyed;
                return false;
            }
        }
        return true;
    }

    public bool Remove(Element element)
    {
        if (element.IsDestroyed)
            throw new ElementDestroyedException(element);

        lock (_lock)
        {
            if(_elements.Remove(element))
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
        lock (_lock)
            return new List<Element>(_elements).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public void Dispose()
    {
        lock(_lock)
        {
            foreach (var element in _elements)
            {
                element.Destroyed -= HandleDestroyed;
            }
            _elements.Clear();
        }
    }
}
