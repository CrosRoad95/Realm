using Realm.Domain.Components.Object;

namespace Realm.Domain.Components.Elements;

public class DurationBasedHoldInteractionComponent : InteractionComponent
{
    private readonly SemaphoreSlim _semaphore = new(1);
    public Entity? Owner { get; private set; }
    private TaskCompletionSource? _interactionTaskComplectionSource;
    private Task? _interactionTask;

    public event Action<DurationBasedHoldInteractionComponent, Entity, TimeSpan>? InteractionStarted;
    public event Action<DurationBasedHoldInteractionComponent, Entity, bool>? InteractionCompleted;

    public DurationBasedHoldInteractionComponent()
    {

    }

    public async Task<bool> BeginInteraction(Entity owningEntity, TimeSpan timeSpan, CancellationToken cancellationToken = default)
    {
        if (owningEntity == null)
            throw new NullReferenceException(nameof(owningEntity));

        if (!await _semaphore.WaitAsync(4, cancellationToken))
            return false;

        try
        {
            if (Owner != null)
                return false;

            Owner = owningEntity;
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

        InteractionStarted?.Invoke(this, owningEntity, timeSpan);
        try
        {
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
        catch (Exception)
        {
            throw;
        }
        finally
        {
            InteractionCompleted?.Invoke(this, owningEntity, false);
        }
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

    public bool EndInteraction(Entity owningEntity)
    {
        if (owningEntity == null)
            throw new NullReferenceException(nameof(owningEntity));

        _semaphore.Wait();

        try
        {
            if (Owner != owningEntity)
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
            InteractionCompleted?.Invoke(this, owningEntity, true);
            _semaphore.Release();
        }
    }
}
