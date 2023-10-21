namespace RealmCore.Server.Exceptions;

public class PrivateVehicleNotFoundException : Exception
{
    public PrivateVehicleNotFoundException(string message) : base(message) { }
    public PrivateVehicleNotFoundException() { }
}
