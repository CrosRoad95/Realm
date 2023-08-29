namespace RealmCore.Server.Rules;

public sealed class MustBePlayerWithLicenseRule : IEntityRule
{
    private readonly int _licenseId;
    private readonly IDateTimeProvider _dateTimeProvider;

    public MustBePlayerWithLicenseRule(int licenseId, IDateTimeProvider dateTimeProvider)
    {
        _licenseId = licenseId;
        _dateTimeProvider = dateTimeProvider;
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
