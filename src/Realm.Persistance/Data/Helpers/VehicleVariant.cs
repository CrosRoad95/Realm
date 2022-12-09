namespace Realm.Persistance.Data.Helpers;

public class VehicleVariant
{
    public byte Variant1 { get; set; } = 255;
    public byte Variant2 { get; set; } = 255;

    public string Serialize()
    {
        return JsonConvert.SerializeObject(this, Formatting.None);
    }

    public static VehicleVariant CreateFromString(string json)
    {
        return JsonConvert.DeserializeObject<VehicleVariant>(json) ?? throw new Exception("Failed to create VehicleVariant from string json");
    }
}
