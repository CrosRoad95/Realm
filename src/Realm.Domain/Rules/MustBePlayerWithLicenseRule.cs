﻿namespace Realm.Domain.Rules;

public sealed class MustBePlayerWithLicenseRule : IEntityRule
{
    private readonly int _licenseId;

    public MustBePlayerWithLicenseRule(int licenseId)
    {
        _licenseId = licenseId;
    }

    public bool Check(Entity entity)
    {
        if (entity.Tag != Entity.EntityTag.Player)
            return false;

        if(entity.TryGetComponent(out LicensesComponent licensesComponent))
        {
            return licensesComponent.HasLicense(_licenseId);
        }
        return false;
    }
}