namespace RealmCore.Persistence;

public class IdentityConfiguration
{
    public class IdentityPolicy
    {
        public string[] RequireRoles { get; set; }
        public Dictionary<string, string> RequireClaims { get; set; }
    }
    public Dictionary<string, IdentityPolicy> Policies { get; set; }
}
