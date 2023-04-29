namespace RealmCore.Server.Interfaces;

public interface ISpawnMarkersService
{
    IReadOnlyList<SpawnMarker> SpawnMarkers { get; }

    void AddSpawnMarker(SpawnMarker spawnMarker);
}
