﻿namespace Realm.Persistance;

public interface IDb
{
    DbSet<User> Users { get; }
    DbSet<Role> Roles { get; }
    DbSet<IdentityUserClaim<Guid>> UserClaims { get; }
    DbSet<IdentityUserLogin<Guid>> UserLogins { get; }
    DbSet<IdentityRoleClaim<Guid>> RoleClaims { get; }
    DbSet<IdentityUserToken<Guid>> UserTokens { get; }
    DbSet<UserLicense> UserLicenses { get; }
    DbSet<Vehicle> Vehicles { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}