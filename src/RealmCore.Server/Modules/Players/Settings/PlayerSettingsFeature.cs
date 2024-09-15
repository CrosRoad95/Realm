namespace RealmCore.Server.Modules.Players.Settings;

public sealed class PlayerSettingsFeature : ElementSettingsFeature<UserSettingData>, IPlayerFeature, IUsesUserPersistentData
{
    public RealmPlayer Player { get; init; }
    public PlayerSettingsFeature(PlayerContext playerContext)
    {
        Player = playerContext.Player;
    }

    public void LogIn(UserData userData)
    {
        Load(userData.Settings);
    }
}
