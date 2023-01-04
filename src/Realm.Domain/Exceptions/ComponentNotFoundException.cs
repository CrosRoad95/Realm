namespace Realm.Domain.Exceptions;

public class ComponentNotFoundException<T> : Exception
{
    public ComponentNotFoundException() : base($"Component '{nameof(T)}' not found")
    { }
}
