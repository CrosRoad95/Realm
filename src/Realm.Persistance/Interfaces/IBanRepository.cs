namespace Realm.Persistance.Interfaces;

public interface IBanRepository : IRepositoryBase
{
    BanData CreateBanForSerial(string serial, DateTime? until = null, string? reason = null, string? responsible = null, int type = 0);
    BanData CreateBanForUser(int userId, DateTime? until = null, string? reason = null, string? responsible = null, int type = 0);
    Task<List<BanData>> GetBansByUserId(int userId, DateTime now);
    Task<List<BanData>> GetBansBySerial(string serial, DateTime now);
    void RemoveBan(BanData ban);
    Task<BanData?> GetBanBySerialAndBanType(string serial, int banType, DateTime now);
}
