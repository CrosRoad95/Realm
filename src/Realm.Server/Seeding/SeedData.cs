using FluentValidation;

namespace Realm.Server.Seeding;

internal class SeedData
{
    public class Spawn
    {
        public string Name { get; set; }
        public Vector3 Position { get; set; }
        public Vector3 Rotation { get; set; }
    }

    public class Blip
    {
        public int Icon { get; set; }
        public Vector3 Position { get; set; }
    }

    public class Pickup
    {
        public ushort Model { get; set; }
        public Vector3 Position { get; set; }
    }

    public class FractionMember
    {
        public string[]? Permissions { get; set; }
    }

    public class Fraction
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public Vector3 Position { get; set; }
        public Dictionary<string, FractionMember> Members { get; set; }
    }

    public class Account
    {
        public string Password { get; set; }
        public Dictionary<string, string> Claims { get; set; }
        public List<string> Roles { get; set; }
    }

    public class VehicleUpgradeDescription
    {
        public float[] MaxVelocity { get; set; }
        public float[] EngineAcceleration { get; set; }
    }

    public Dictionary<string, Spawn> Spawns = new();
    public Dictionary<string, Blip> Blips = new();
    public Dictionary<string, Pickup> Pickups = new();
    public Dictionary<string, Fraction> Fractions = new();
    public List<string> Roles = new();
    public Dictionary<string, Account> Accounts = new();
    public Dictionary<string, VehicleUpgradeDescription> Upgrades = new();
}

internal class SeedValidator : AbstractValidator<SeedData>
{
    public SeedValidator()
    {

    }
}