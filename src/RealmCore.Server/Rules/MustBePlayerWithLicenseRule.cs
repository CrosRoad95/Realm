namespace RealmCore.Server.Rules;

public sealed class MustBePlayerWithLicenseRule : IElementRule
{
    private readonly int _licenseId;
    private readonly IDateTimeProvider _dateTimeProvider;

    public MustBePlayerWithLicenseRule(int licenseId, IDateTimeProvider dateTimeProvider)
    {
        _licenseId = licenseId;
        _dateTimeProvider = dateTimeProvider;
    }

    public bool Check(Element element)
    {
        if(element is RealmPlayer player && player.Components.TryGetComponent(out LicensesComponent licensesComponent))
            return licensesComponent.HasLicense(_licenseId);
        return false;
    }
}
