namespace RealmCore.Server.Modules.Domain;

public record Location(Vector3 Position, Vector3 Rotation = default, byte? Interior = null, ushort? Dimension = null)
{
    public Location(Vector3 Position, byte? Interior = null, ushort? Dimension = null) : this(Position, default, Interior, Dimension) { }
    public Location(Vector3 Position) : this(Position, default, null, null) { }
    public Location(float x, float y, float z) : this(new Vector3(x, y, z), default, null, null) { }
    public Location() : this(default, default, null, null) { }

    private static Location _location = new();
    public static Location Zero => _location;
}