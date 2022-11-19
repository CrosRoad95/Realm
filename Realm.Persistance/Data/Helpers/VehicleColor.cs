using Newtonsoft.Json;
using System.Drawing;

namespace Realm.Persistance.Data.Helpers;

public class VehicleColor
{
    public Color Color1 { get; set; } = Color.White;
    public Color Color2 { get; set; } = Color.White;
    public Color Color3 { get; set; } = Color.White;
    public Color Color4 { get; set; } = Color.White;
    public Color HeadLightColor { get; set; } = Color.White;

    public string Serialize()
    {
        return JsonConvert.SerializeObject(this, Formatting.None);
    }

    public static VehicleColor CreateFromString(string json)
    {
        return JsonConvert.DeserializeObject<VehicleColor>(json) ?? throw new Exception("Failed to create VehicleColor from string json");
    }
}
