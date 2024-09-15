namespace RealmCore.Persistence.Data;

public class MapData
{
    public int Id { get; set; }
    public int LocationId { get; set; }
    public bool Loaded { get; set; }
    public int FileUploadId { get; set; }
    public string? Metadata { get; set; }

    public ICollection<MapUserData> MapsUsers { get; set; } = [];
    public UploadFileData UploadFile { get; set; }
}

public class MapUserData
{
    public int MapId { get; set; }
    public int UserId { get; set; }

    public MapData Map { get; set; }
    public UserData User { get; set; }
}