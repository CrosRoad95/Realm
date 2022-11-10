namespace Realm.MTARPGServer;

internal class SeedData
{
    public class Spawn
    {
        public string Name { get; set; }
        public Vector3 Position { get; set; }
        public Vector3 Rotation { get; set; }
    }
    
    public class Account
    {
        public string Password { get; set; }
        public Dictionary<string, string> Claims { get; set; }
        public List<string> Roles { get; set; }
    }

    public Dictionary<string, Spawn> Spawns = new();
    public List<string> Roles = new();
    public Dictionary<string, Account> Accounts = new();
}

internal class SeedValidator : AbstractValidator<SeedData>
{
    public SeedValidator()
    {

    }
}