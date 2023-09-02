namespace RealmCore.Persistence.Repository;

internal sealed class UserWhitelistedSerialsRepository : IUserWhitelistedSerialsRepository
{
    private readonly IDb _db;

    public UserWhitelistedSerialsRepository(IDb db)
    {
        _db = db;
    }

    public Task<bool> IsSerialWhitelisted(int userId, string serial)
    {
        var query = _db.UserWhitelistedSerials
            .TagWith(nameof(UserWhitelistedSerialsRepository))
            .Where(x => x.UserId == userId && x.Serial == serial);
        return query.AnyAsync();
    }

    public async Task<bool> TryAddWhitelistedSerial(int userId, string serial)
    {
        if (serial.Length != 32)
            throw new ArgumentException(null, nameof(serial));

        try
        {
            _db.UserWhitelistedSerials.Add(new UserWhitelistedSerialData
            {
                Serial = serial,
                UserId = userId
            });

            var added = await _db.SaveChangesAsync();
            return added > 0;
        }
        catch (Exception)
        {
            return false;
        }
        finally
        {
            _db.ChangeTracker.Clear();
        }
    }

    public async Task<bool> TryRemoveWhitelistedSerial(int userId, string serial)
    {
        if (serial.Length != 32)
            throw new ArgumentException(null, nameof(serial));

        try
        {
            var deleted = await _db.UserWhitelistedSerials
                .TagWith(nameof(UserWhitelistedSerialsRepository))
                .Where(x => x.UserId == userId && x.Serial == serial)
                .ExecuteDeleteAsync();

            return deleted > 0;
        }
        catch (Exception)
        {
            return false;
        }
        finally
        {
            _db.ChangeTracker.Clear();
        }
    }
}
