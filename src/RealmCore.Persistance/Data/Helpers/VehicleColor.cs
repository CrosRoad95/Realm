namespace RealmCore.Persistence.Data.Helpers;

public class VehicleColor
{
    public Color Color1 { get; set; } = Color.White;
    public Color Color2 { get; set; } = Color.White;
    public Color Color3 { get; set; } = Color.White;
    public Color Color4 { get; set; } = Color.White;
    public Color HeadLightColor { get; set; } = Color.White;

    public VehicleColor() { }
    public VehicleColor(Color color1, Color color2, Color color3, Color color4, Color headLightColor)
    {
        Color1 = color1;
        Color2 = color2;
        Color3 = color3;
        Color4 = color4;
        HeadLightColor = headLightColor;
    }

    public string Serialize()
    {
        return JsonConvert.SerializeObject(this, Formatting.None);
    }

    public static VehicleColor CreateFromString(string json)
    {
        return JsonConvert.DeserializeObject<VehicleColor>(json) ?? throw new Exception("Failed to create VehicleColor from string json");
    }
}
