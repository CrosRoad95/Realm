namespace Realm.Domain.Exceptions;

public class EntityAlreadyExistsException : Exception
{
    public EntityAlreadyExistsException(string name)
        : base($"Entity with name '{name}' already exists.")
    {
        Name = name;
    }

    public string Name { get; }
}
