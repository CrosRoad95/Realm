using Newtonsoft.Json.Linq;

namespace RealmCore.Persistence.Repository;

public sealed class BusinessesRepository
{
    private readonly IDb _db;

    public BusinessesRepository(IDb db)
    {
        _db = db;
    }

    public async Task<BusinessData> Create(int category, string? metadata = null, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(Create));

        if (activity != null)
        {
            activity.AddTag("Category", category);
        }

        var business = new BusinessData
        {
            Category = category,
            Metadata = metadata,
        };

        try
        {
            _db.Businesses.Add(business);
            await _db.SaveChangesAsync(cancellationToken);
        }
        finally
        {
            _db.ChangeTracker.Clear();
        }

        return business;
    }

    public async Task<bool> AddTimeBaseOperationGroup(int businessId, int groupId, string? metadata = null, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(AddTimeBaseOperationGroup));

        if (activity != null)
        {
            activity.AddTag("BusinessId", businessId);
            activity.AddTag("GroupId", groupId);
        }

        var timeBaseOperationGroupBusiness = new TimeBaseOperationGroupBusinessData
        {
            BusinessId = businessId,
            OperationGroupId = groupId
        };

        try
        {
            _db.TimeBaseOperationGroupBusinesses.Add(timeBaseOperationGroupBusiness);
            await _db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            return false;
        }
        finally
        {
            _db.ChangeTracker.Clear();
        }

        return true;
    }

    public async Task<bool> AddUser(int businessId, int userId, string? metadata = null, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(AddUser));

        if (activity != null)
        {
            activity.AddTag("BusinessId", businessId);
            activity.AddTag("UserId", userId);
        }

        var businessUser = new BusinessUserData
        {
            BusinessId = businessId,
            UserId = userId,
            Metadata = metadata
        };

        try
        {
            _db.BusinessesUsers.Add(businessUser);
            await _db.SaveChangesAsync(cancellationToken);
        }
        catch(DbUpdateException)
        {
            return false;
        }
        finally
        {
            _db.ChangeTracker.Clear();
        }

        return true;
    }

    public async Task<BusinessData?> GetById(int businessId, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(GetById));

        if (activity != null)
        {
            activity.AddTag("BusinessId", businessId);
        }


        var query = _db.Businesses
            .TagWithSource(nameof(BusinessesRepository))
            .AsNoTrackingWithIdentityResolution()
            .Where(x => x.Id == businessId);

        return await query.FirstOrDefaultAsync(cancellationToken);
    }
    
    public async Task<int[]> GetUsersById(int businessId, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(GetUsersById));

        if (activity != null)
        {
            activity.AddTag("BusinessId", businessId);
        }


        var query = _db.Businesses
            .TagWithSource(nameof(BusinessesRepository))
            .AsNoTrackingWithIdentityResolution()
            .Where(x => x.Id == businessId)
            .SelectMany(x => x.Users.Select(y => y.UserId));

        return await query.ToArrayAsync(cancellationToken);
    }
    
    public async Task<BusinessData[]> GetByUserId(int userId, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(GetByUserId));

        if (activity != null)
        {
            activity.AddTag("UserId", userId);
        }

        var query = _db.Businesses
            .TagWithSource(nameof(BusinessesRepository))
            .AsNoTrackingWithIdentityResolution()
            .Where(x => x.Users.Any(y => y.UserId == userId));

        return await query.ToArrayAsync(cancellationToken);
    }
    
    public async Task<BusinessData[]> GetByUserIdAndCategory(int userId, int categoryId, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(GetByUserIdAndCategory));

        if (activity != null)
        {
            activity.AddTag("UserId", userId);
        }

        var query = _db.Businesses
            .TagWithSource(nameof(BusinessesRepository))
            .AsNoTrackingWithIdentityResolution()
            .Where(x => x.Users.Any(y => y.UserId == userId) && x.Category == categoryId);

        return await query.ToArrayAsync(cancellationToken);
    }
    
    public async Task<bool> SetMetadata(int businessId, string? metadata, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(SetMetadata));

        if (activity != null)
        {
            activity.AddTag("BusinessId", businessId);
        }

        var query = _db.Businesses
            .TagWithSource(nameof(BusinessesRepository))
            .AsNoTrackingWithIdentityResolution()
            .Where(x => x.Id == businessId);

        return await query.ExecuteUpdateAsync(x => x.SetProperty(y => y.Metadata, metadata), cancellationToken) == 1;
    }
    
    public async Task<bool> SetUserMetadata(int businessId, int userId, string? metadata, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(SetUserMetadata));

        if (activity != null)
        {
            activity.AddTag("BusinessId", businessId);
            activity.AddTag("UserId", userId);
        }

        var query = _db.BusinessesUsers
            .TagWithSource(nameof(BusinessesRepository))
            .AsNoTrackingWithIdentityResolution()
            .Where(x => x.BusinessId == businessId && x.UserId == userId);

        return await query.ExecuteUpdateAsync(x => x.SetProperty(y => y.Metadata, metadata), cancellationToken) == 1;
    }

    public async Task<bool> IncreaseStatistic(int businessId, int statisticId, float value, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(IncreaseStatistic));
        if (value < 0)
            return false;

        var query = _db.BusinessStatistics
            .TagWithSource(nameof(BusinessesRepository))
            .Where(x => x.BusinessId == businessId && x.StatisticId == statisticId);

        var statistic = await query.FirstOrDefaultAsync(cancellationToken);
        
        try
        {
            if (statistic == null)
            {
                _db.BusinessStatistics.Add(new BusinessStatisticData
                {
                    BusinessId = businessId,
                    StatisticId = statisticId,
                    Value = value
                });
            }
            else
            {
                statistic.Value += value;
            }
            await _db.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (DbUpdateException)
        {
            return false;
        }
        finally
        {
            _db.ChangeTracker.Clear();
        }
    }

    public async Task<IReadOnlyDictionary<int, float>> GetStatistics(int businessId, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(GetStatistics));

        if (activity != null)
        {
            activity.AddTag("BusinessId", businessId);
        }

        var query = _db.BusinessStatistics
            .TagWithSource(nameof(BusinessesRepository))
            .AsNoTrackingWithIdentityResolution()
            .Where(x => x.BusinessId == businessId);

        return await query.ToDictionaryAsync(x => x.StatisticId, y => y.Value, cancellationToken);
    }

    public async Task<bool> GiveMoney(int businessId, decimal amount, CancellationToken cancellationToken = default)
    {
        if(amount < 0)
            throw new ArgumentOutOfRangeException(nameof(amount));

        using var activity = Activity.StartActivity(nameof(GiveMoney));

        if (activity != null)
        {
            activity.AddTag("BusinessId", businessId);
            activity.AddTag("Amount", amount);
        }

        var query = _db.Businesses
            .TagWithSource(nameof(BusinessesRepository))
            .Where(x => x.Id == businessId);

        var businessData = await query.FirstOrDefaultAsync(cancellationToken);
        if (businessData == null)
            return false;

        try
        {
            businessData.Money += amount;
            await _db.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (DbUpdateException)
        {
            return false;
        }
        finally
        {
            _db.ChangeTracker.Clear();
        }
    }

    public async Task<bool> TakeMoney(int businessId, decimal amount, CancellationToken cancellationToken = default)
    {
        if(amount < 0)
            throw new ArgumentOutOfRangeException(nameof(amount));

        using var activity = Activity.StartActivity(nameof(TakeMoney));

        if (activity != null)
        {
            activity.AddTag("BusinessId", businessId);
            activity.AddTag("Amount", amount);
        }

        var query = _db.Businesses
            .TagWithSource(nameof(BusinessesRepository))
            .Where(x => x.Id == businessId);

        var businessData = await query.FirstOrDefaultAsync(cancellationToken);
        if (businessData == null)
            return false;

        try
        {
            businessData.Money -= amount;
            if (businessData.Money < 0)
                return false;

            await _db.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (DbUpdateException)
        {
            return false;
        }
        finally
        {
            _db.ChangeTracker.Clear();
        }
    }

    public static readonly ActivitySource Activity = new("RealmCore.BusinessesRepository", "1.0.0");
}
