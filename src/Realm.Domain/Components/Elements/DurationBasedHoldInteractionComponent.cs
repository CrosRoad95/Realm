using Realm.Domain.Components.Object;

namespace Realm.Domain.Components.Elements;

public class DurationBasedHoldInteractionComponent : InteractionComponent
{
    [Inject]
    private ILogger<DurationBasedHoldInteractionComponent> Logger { get; set; }

    private readonly SemaphoreSlim _semaphore = new(1);
    public Entity? Owner { get; private set; }
    private TaskCompletionSource? _interactionTaskComplectionSource;
    private Task? _interactionTask;

    public DurationBasedHoldInteractionComponent()
    {

    }

    public async Task<bool> BeginInteraction(Entity playerEntity, TimeSpan timeSpan, CancellationToken cancellationToken = default)
    {
        if (playerEntity == null)
            throw new NullReferenceException(nameof(playerEntity));

        if (!await _semaphore.WaitAsync(4))
            return false;

        try
        {
            if (Owner != null)
                return false;

            Owner = playerEntity;
            Owner.Destroyed += HandleDestroyed;

            _interactionTaskComplectionSource = new TaskCompletionSource();
            _interactionTask = Task.Delay(timeSpan, cancellationToken);
        }
        catch (Exception)
        {
            throw;
        }
        finally
        {
            _semaphore.Release();
        }

        var finishedTask = await Task.WhenAny(_interactionTaskComplectionSource.Task, _interactionTask);
        if (finishedTask == _interactionTask)
        {
            Owner.Destroyed -= HandleDestroyed;
            Owner = null;
            cancellationToken.ThrowIfCancellationRequested();
            return true;
        }
        return false;
    }

    private void HandleDestroyed(Entity _)
    {
        _semaphore.Wait();
        try
        {

            if (_interactionTaskComplectionSource != null)
            {
                _interactionTaskComplectionSource.SetCanceled();
                _interactionTaskComplectionSource = null;
                _interactionTask = null;
            }
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public bool EndInteraction(Entity playerEntity)
    {
        if (playerEntity == null)
            throw new NullReferenceException(nameof(playerEntity));

        _semaphore.Wait();

        try
        {
            if (Owner != playerEntity)
                return false;

            Owner.Destroyed -= HandleDestroyed;
            Owner = null;

            if (_interactionTaskComplectionSource != null)
            {
                _interactionTaskComplectionSource.SetCanceled();
                _interactionTaskComplectionSource = null;
            }

            return true;
        }
        catch(Exception)
        {
            throw;
        }
        finally
        {
            _semaphore.Release();
        }
    }
}
