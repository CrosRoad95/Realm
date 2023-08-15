namespace RealmCore.Server.Rules;

public sealed class MustBePlayerWithLicenseRule : IEntityRule
{
    private readonly int _licenseId;

    public MustBePlayerWithLicenseRule(int licenseId)
    {
        _licenseId = licenseId;
    }

    public bool Check(Entity entity)
    {
        if (!entity.HasComponent<PlayerTagComponent>())
            return false;

        if (entity.TryGetComponent(out LicensesComponent licensesComponent))
        {
            return licensesComponent.HasLicense(_licenseId);
        }
        return false;
    }
}
