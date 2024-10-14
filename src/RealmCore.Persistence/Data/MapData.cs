namespace RealmCore.Persistence.Data;

public class MapData
{
    public int Id { get; set; }
    public int LocationId { get; set; }
    public bool Loaded { get; set; }
    public int FileUploadId { get; set; }
    public string? Metadata { get; set; }

    public ICollection<MapUserData> MapsUsers { get; set; } = [];
    public UploadFileData? UploadFile { get; set; }
}

public class MapUserData
{
    public int MapId { get; set; }
    public int UserId { get; set; }

    public MapData Map { get; set; }
    public UserData User { get; set; }
}

public class UserTimeBasedRewards
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int Category { get; set; }
    public decimal Money { get; set; }
    public decimal Experience { get; set; }
    public decimal MoneyPerHour { get; set; }
    public decimal ExperiencePerHour { get; set; }
    public DateTime? LastUpdate { get; set; }
    public DateTime CreatedAt { get; set; }

    public UserData? User { get; set; }
}