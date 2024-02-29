namespace RealmCore.Server.Modules.Commands;

public class CommandArgumentException : Exception
{
    public int Index { get; }
    public string? Argument { get; }

    internal CommandArgumentException(int index, string? message, string? argument) : base(message)
    {
        Index = index + 1;
        Argument = argument;
    }
}
