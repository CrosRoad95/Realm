namespace RealmCore.Persistence.Data.Helpers;

public class VehicleDoorOpenRatio
{
    public float Hood { get; set; }
    public float Trunk { get; set; }
    public float FrontLeft { get; set; }
    public float FrontRight { get; set; }
    public float RearLeft { get; set; }
    public float RearRight { get; set; }

    public string Serialize()
    {
        return JsonConvert.SerializeObject(this, Formatting.None);
    }

    public static VehicleDoorOpenRatio CreateFromString(string json)
    {
        return JsonConvert.DeserializeObject<VehicleDoorOpenRatio>(json) ?? throw new Exception("Failed to create VehicleDoorOpenRation from string json");
    }
}
