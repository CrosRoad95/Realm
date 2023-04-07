namespace RealmCore.Server.Exceptions;

public class EntityAlreadyExistsException : Exception
{
    public EntityAlreadyExistsException(string name)
        : base($"Entity with name '{name}' already exists.")
    {
        Name = name;
    }

    public string Name { get; }
}
