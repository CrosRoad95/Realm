namespace RealmCore.Server.Exceptions;

public class ElementDestroyedException : Exception
{
    public ElementDestroyedException(string message) : base(message) { }
    public ElementDestroyedException() : base() { }
}
