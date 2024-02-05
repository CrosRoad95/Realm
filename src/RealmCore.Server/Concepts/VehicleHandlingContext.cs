namespace RealmCore.Server.Concepts;

public delegate void HandlingDelegate(VehicleHandlingContext vehicleHandlingContext);
public delegate void ModifyHandlingDelegate(ref VehicleHandling vehicleHandling);

public class VehicleHandlingContext
{
    private VehicleHandling _vehicleHandling;

    internal VehicleHandling VehicleHandling => _vehicleHandling;

    public VehicleHandlingContext(ushort model)
    {
        _vehicleHandling = VehicleHandlingConstants.DefaultVehicleHandling[model];
    }

    public void Modify(ModifyHandlingDelegate modifyHandlingDelegate)
    {
        modifyHandlingDelegate(ref _vehicleHandling);
    }
}

public interface IVehicleHandlingModifier
{
    void Apply(VehicleHandlingContext context, HandlingDelegate next);
}

public class EmptyVehicleHandlingModifier : IVehicleHandlingModifier
{
    private EmptyVehicleHandlingModifier() { }
    public static IVehicleHandlingModifier Instance { get; } = new EmptyVehicleHandlingModifier();
    public void Apply(VehicleHandlingContext context, HandlingDelegate next)
    {
        next(context);
    }
}
