namespace RealmCore.Server.Rules;

public sealed class MustBePlayerWithLicenseRule : IElementRule
{
    private readonly int _licenseId;

    public MustBePlayerWithLicenseRule(int licenseId)
    {
        _licenseId = licenseId;
    }

    public bool Check(Element element)
    {
        if(element is RealmPlayer player)
            return player.Licenses.Has(_licenseId);
        return false;
    }
}
