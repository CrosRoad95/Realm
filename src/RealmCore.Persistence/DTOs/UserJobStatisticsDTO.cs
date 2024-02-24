namespace RealmCore.Persistence.DTOs;

public class UserJobStatisticsDto
{
    public int UserId { get; set; }
    public int JobId { get; set; }
    public ulong Points { get; set; }
    public ulong TimePlayed { get; set; }
}
