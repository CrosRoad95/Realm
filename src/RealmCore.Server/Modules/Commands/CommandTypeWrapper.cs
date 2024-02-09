namespace RealmCore.Server.Modules.Commands;

internal class CommandTypeWrapper
{
    public Type Type { get; }
    public CommandTypeWrapper(Type type)
    {
        Type = type;
    }
}
