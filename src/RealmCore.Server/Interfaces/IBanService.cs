namespace RealmCore.Server.Interfaces;

public interface IBanService
{
    Task BanSerial(string serial, DateTime? until = null, string? reason = null, string? responsible = null, int type = 0);
    Task BanUserId(int userId, DateTime? until = null, string? reason = null, string? responsible = null, int type = 0);
    Task<BanData?> GetBanBySerialAndBanType(string serial, int banType);
    Task<List<BanData>> GetBansBySerial(string serial);
    Task<List<BanData>> GetBansByUserId(int userId);
    Task RemoveBan(BanData ban);
}
