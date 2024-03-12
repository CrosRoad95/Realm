namespace RealmCore.Persistence.Repository;

public interface IUserWhitelistedSerialsRepository
{
    Task<bool> IsSerialWhitelisted(int userId, string serial, CancellationToken cancellationToken = default);
    Task<bool> TryAddWhitelistedSerial(int userId, string serial, CancellationToken cancellationToken = default);
    Task<bool> TryRemoveWhitelistedSerial(int userId, string serial, CancellationToken cancellationToken = default);
}

internal sealed class UserWhitelistedSerialsRepository : IUserWhitelistedSerialsRepository
{
    private readonly IDb _db;

    public UserWhitelistedSerialsRepository(IDb db)
    {
        _db = db;
    }

    public async Task<bool> IsSerialWhitelisted(int userId, string serial, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(IsSerialWhitelisted));

        var query = _db.UserWhitelistedSerials
            .AsNoTracking()
            .TagWith(nameof(UserWhitelistedSerialsRepository))
            .Where(x => x.UserId == userId && x.Serial == serial);
        return await query.AnyAsync(cancellationToken);
    }

    public async Task<bool> TryAddWhitelistedSerial(int userId, string serial, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(TryAddWhitelistedSerial));

        if (serial.Length != 32)
            throw new ArgumentException(null, nameof(serial));

        try
        {
            _db.UserWhitelistedSerials.Add(new UserWhitelistedSerialData
            {
                Serial = serial,
                UserId = userId
            });

            var added = await _db.SaveChangesAsync(cancellationToken);
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

    public async Task<bool> TryRemoveWhitelistedSerial(int userId, string serial, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(TryRemoveWhitelistedSerial));

        if (serial.Length != 32)
            throw new ArgumentException(null, nameof(serial));

        try
        {
            var deleted = await _db.UserWhitelistedSerials
                .AsNoTracking()
                .TagWith(nameof(UserWhitelistedSerialsRepository))
                .Where(x => x.UserId == userId && x.Serial == serial)
                .ExecuteDeleteAsync(cancellationToken);

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

    public static readonly ActivitySource Activity = new("RealmCore.UserWhitelistedSerialsRepository", "1.0.0");
}
