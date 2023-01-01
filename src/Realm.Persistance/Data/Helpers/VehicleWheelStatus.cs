namespace Realm.Persistance.Data.Helpers;

public class VehicleWheelStatus
{
    public enum WheelStatus
    {
        Inflated,
        Flat,
        FallenOff,
        Collisionless
    }

    public WheelStatus FrontLeft { get; set; }
    public WheelStatus RearLeft { get; set; }
    public WheelStatus FrontRight { get; set; }
    public WheelStatus RearRight { get; set; }


    public string Serialize()
    {
        return JsonConvert.SerializeObject(this, Formatting.None);
    }

    public static VehicleWheelStatus CreateFromString(string json)
    {
        return JsonConvert.DeserializeObject<VehicleWheelStatus>(json) ?? throw new Exception("Failed to create VehicleWheelStatus from string json");
    }
}
