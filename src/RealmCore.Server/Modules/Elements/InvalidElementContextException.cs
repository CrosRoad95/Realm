namespace RealmCore.Server.Modules.Elements;

public class InvalidElementContextException : Exception
{
    public InvalidElementContextException() : base("ElementContext has no element set") { }
}
