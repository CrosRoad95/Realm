namespace RealmCore.Server.Modules.Commands;

public class CommandArgumentException : Exception
{
    public int? Index { get; init; }
    public string? Argument { get; init; }

    internal CommandArgumentException(int? index, string? message, string? argument) : base(message)
    {
        Index = index + 1;
        Argument = argument;
    }
}
