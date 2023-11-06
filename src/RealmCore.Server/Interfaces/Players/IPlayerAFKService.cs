

namespace RealmCore.Server.Interfaces.Players;

public interface IPlayerAFKService
{
    DateTime? LastAFK { get; }
    bool IsAFK { get; }

    event Action<IPlayerAFKService, bool, TimeSpan>? StateChanged;

    CancellationToken CreateCancellationToken(bool? expectedAfkState = null);
    internal void HandleAFKStarted();
    internal void HandleAFKStopped();
}
