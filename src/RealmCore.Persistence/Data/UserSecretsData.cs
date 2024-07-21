namespace RealmCore.Persistence.Data;

public sealed class UserSecretsData
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int GroupId { get; set; }
    public int SecretId { get; set; }
    public DateTime CreatedAt { get; set; }
}