namespace RealmCore.Server.Exceptions;

public class EntityAlreadyExistsException : Exception
{
    public string Name { get; }

    public EntityAlreadyExistsException(string name)
        : base($"Entity with name '{name}' already exists.")
    {
        Name = name;
    }
}
