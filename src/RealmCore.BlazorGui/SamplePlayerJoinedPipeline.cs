namespace RealmCore.BlazorGui;

public class SamplePlayerJoinedPipeline : IPlayerJoinedPipeline
{
    public async Task<bool> Next(RealmPlayer player)
    {
        //throw new NotImplementedException();
        return true;
    }
}