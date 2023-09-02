namespace RealmCore.Persistence.Interfaces;

public interface IUserWhitelistedSerialsRepository
{
    Task<bool> IsSerialWhitelisted(int userId, string serial);
    Task<bool> TryAddWhitelistedSerial(int userId, string serial);
    Task<bool> TryRemoveWhitelistedSerial(int userId, string serial);
}
