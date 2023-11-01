namespace RealmCore.Server.Exceptions;

public class ComponentNotFoundException<T> : Exception
{
    public ComponentNotFoundException() : base($"Component '{typeof(T).Name}' not found")
    { }
}