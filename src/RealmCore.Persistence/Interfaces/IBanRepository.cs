namespace RealmCore.Persistence.Interfaces;

public interface IBanRepository
{
    Task<BanData> CreateBanForSerial(string serial, DateTime? until = null, string? reason = null, string? responsible = null, int type = 0);
    Task<BanData> CreateBanForUser(int userId, DateTime? until = null, string? reason = null, string? responsible = null, int type = 0);
    Task<List<BanData>> GetBansByUserId(int userId, DateTime now);
    Task<List<BanData>> GetBansBySerial(string serial, DateTime now);
    Task<bool> Delete(int banId);
    Task<bool> DeleteByUserId(int userId, int type = 0);
    Task<bool> DeleteBySerial(string serial, int type = 0);
    Task<List<BanData>> GetBansByUserIdOrSerial(int userId, string serial, DateTime now);
    Task<BanData?> GetBanBySerialAndType(string serial, int type, DateTime now);
}
