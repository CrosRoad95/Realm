namespace RealmCore.Persistence.Interfaces;

public interface IUserWhitelistedSerialsRepository
{
    Task<bool> IsSerialWhitelisted(int userId, string serial, CancellationToken cancellationToken = default);
    Task<bool> TryAddWhitelistedSerial(int userId, string serial, CancellationToken cancellationToken = default);
    Task<bool> TryRemoveWhitelistedSerial(int userId, string serial, CancellationToken cancellationToken = default);
}
