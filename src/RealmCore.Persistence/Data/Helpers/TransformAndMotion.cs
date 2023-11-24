namespace RealmCore.Persistence.Data.Helpers;

public sealed class TransformAndMotion
{
    public Vector3 Position { get; set; }
    public Vector3 Rotation { get; set; }
    public byte Interior { get; set; }
    public ushort Dimension { get; set; }
    public Vector3 Velocity { get; set; }
    public Vector3 AngularVelocity { get; set; }

    public string Serialize()
    {
        return JsonConvert.SerializeObject(this, Formatting.None);
    }

    public static TransformAndMotion CreateFromString(string json)
    {
        if (string.IsNullOrEmpty(json))
            return new();

        return JsonConvert.DeserializeObject<TransformAndMotion>(json) ?? throw new Exception("Failed to create TransformAndMotion from string json");
    }
}
