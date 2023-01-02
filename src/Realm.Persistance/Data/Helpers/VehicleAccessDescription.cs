namespace Realm.Persistance.Data.Helpers;

public class VehicleAccessDescription
{
    public bool Ownership { get; set; }

    public string Serialize()
    {
        return JsonConvert.SerializeObject(this, Formatting.None);
    }

    public static VehicleAccessDescription CreateFromString(string json)
    {
        return JsonConvert.DeserializeObject<VehicleAccessDescription>(json) ?? throw new Exception("Failed to create VehicleAccessDescription from string json");
    }
}
