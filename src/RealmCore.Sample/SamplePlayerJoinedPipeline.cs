
namespace RealmCore.Sample;

public class SamplePlayerJoinedPipeline : IPlayerJoinedPipeline
{
    public async Task<bool> Next(Player player)
    {
        //throw new NotImplementedException();
        return true;
    }
}