namespace RealmCore.Server.Exceptions;

public class CommandArgumentException : Exception
{
    public int Index { get; }
    public string? Message { get; }
    public string? Argument { get; }

    internal CommandArgumentException(int index, string? message, string? argument)
    {
        Index = index;
        Message = message;
        Argument = argument;
    }
}
