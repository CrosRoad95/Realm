using Realm.Common.Providers;

namespace Realm.Persistance.Interfaces;

public interface IBanRepository : IRepositoryBase
{
    Ban CreateBanForSerial(string serial, DateTime? until = null, string? reason = null, string? responsible = null, int type = 0);
    Ban CreateBanForUser(int userId, DateTime? until = null, string? reason = null, string? responsible = null, int type = 0);
    Task<List<Ban>> GetBansByUserId(int userId, IDateTimeProvider dateTimeProvider);
    Task<List<Ban>> GetBansBySerial(string serial, IDateTimeProvider dateTimeProvider);
    void RemoveBan(Ban ban);
}
