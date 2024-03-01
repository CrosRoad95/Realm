namespace RealmCore.Server.Modules.Domain;

public record Location(Vector3 Position, Vector3 Rotation = default, byte? Interior = null, ushort? Dimension = null)
{
    public Location(Vector3 Position, byte? Interior = null, ushort? Dimension = null) : this(Position, default, Interior, Dimension) { }
    public Location(Vector3 Position) : this(Position, default, null, null) { }
    public Location(float x, float y, float z) : this(new Vector3(x, y, z), default, null, null) { }
    public Location() : this(default, default, null, null) { }

    private static readonly Location _location = new();
    public static Location Zero => _location;

    public byte GetInteriorOrDefault() => Interior ?? 0;
    public ushort GetDimensionOrDefault() => Dimension ?? 0;

    public bool CompareInteriorAndDimension(Location other)
    {
        return GetInteriorOrDefault() == other.GetInteriorOrDefault() && GetDimensionOrDefault() == other.GetDimensionOrDefault();
    }

    public float DistanceTo(Location other)
    {
        if (!CompareInteriorAndDimension(other))
            return float.MaxValue;

        return (Position - other.Position).Length();
    }

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.Append($"Position: {Position.X:0.00}, {Position.Y:0.00}, {Position.Z:0.00}");
        if (Rotation != Vector3.Zero)
            sb.Append($", Rotation: {Rotation.X:0.00}, {Rotation.Y:0.00}, {Rotation.Z:0.00}");
        if (Interior != null)
            sb.Append($", Interior: {Interior}");
        if (Interior != null)
            sb.Append($", Dimension: {Dimension}");

        return sb.ToString();
    }
}