
namespace RealmCore.Server.Interfaces.Players;

public interface IPlayerLevelService : IPlayerService
{
    uint NextLevelRequiredExperience { get; }
    uint Level { get; set; }
    uint Experience { get; set; }

    event Action<IPlayerLevelService, uint, bool>? LevelChanged;
    event Action<IPlayerLevelService, uint>? ExperienceChanged;

    void GiveExperience(uint amount);
}
