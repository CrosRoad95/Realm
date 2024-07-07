namespace RealmCore.Persistence.Data;

public sealed class UploadFileData
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string FileType { get; set; }
    public ulong Size { get; set; }
    public DateTime? LastAccessedAt { get; set; }
    public string Metadata { get; set; }
    public DateTime UploadedAt { get; set; }

    public ICollection<UserUploadFileData> UserUploadFiles { get; set; } = [];
}

public sealed class UserUploadFileData
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int UploadFileId { get; set; }

    public UserData? User { get; set; }
    public UploadFileData? UploadFile { get; set; }
}