namespace RealmCore.Server.Modules.Seeder;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

internal class SeedData
{
    public class FractionMemberSeedData
    {
        public int Rank { get; set; }
        public string RankName { get; set; }
    }

    public class FractionSeedData
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public Vector3 Position { get; set; }
        public Dictionary<string, FractionMemberSeedData> Members { get; set; }
    }

    public class DiscordIntegrationSeedData
    {
        public ulong UserId { get; set; }
    }

    public class IntegrationsSeedData
    {
        public DiscordIntegrationSeedData? Discord { get; set; }
    }

    public class UserSeedData
    {
        public string Password { get; set; }
        public Dictionary<string, string>? Claims { get; set; }
        public Dictionary<int, string>? Settings { get; set; }
        public List<string>? Roles { get; set; }
        public IntegrationsSeedData? Integrations { get; set; }
    }

    public class VehicleUpgradeDescriptionSeedData
    {
        public float[] MaxVelocity { get; set; }
        public float[] EngineAcceleration { get; set; }
    }

    public class GroupMemberSeedData
    {
        public int Rank { get; set; }
        public string RankName { get; set; }
    }

    public Dictionary<string, FractionSeedData> Fractions = [];
    public Dictionary<string, object> Roles = [];
    public Dictionary<string, UserSeedData> Users = [];
    public Dictionary<string, VehicleUpgradeDescriptionSeedData> Upgrades = [];
}

#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
