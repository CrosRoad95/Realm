namespace Realm.Server.Interfaces;

public interface IBanService
{
    Task BanSerial(string serial, DateTime? until = null, string? reason = null, string? responsible = null, int type = 0);
    Task BanUserId(int userId, DateTime? until = null, string? reason = null, string? responsible = null, int type = 0);
    Task<List<Ban>> GetBansBySerial(string serial);
    Task<List<Ban>> GetBansByUserId(int userId);
    Task RemoveBan(Ban ban);
}
