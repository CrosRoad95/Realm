namespace RealmCore.Server.Modules.Elements.Interactions;

public abstract class DurationBasedHoldInteraction : Interaction
{
    private readonly SemaphoreSlim _semaphore = new(1);
    public RealmPlayer? Owner { get; private set; }
    private TaskCompletionSource? _interactionTaskCompletionSource;
    private Task? _interactionTask;

    public event Action<DurationBasedHoldInteraction, RealmPlayer, TimeSpan, CancellationToken>? InteractionStarted;
    public event Action<DurationBasedHoldInteraction, RealmPlayer, bool>? InteractionCompleted;

    public abstract TimeSpan Time { get; }

    public DurationBasedHoldInteraction()
    {

    }

    public async Task<bool> BeginInteraction(RealmPlayer owningPlayer, CancellationToken cancellationToken = default)
    {
        if (owningPlayer == null)
            throw new NullReferenceException(nameof(owningPlayer));

        if (!await _semaphore.WaitAsync(4, cancellationToken))
            return false;

        try
        {
            if (Owner != null)
                return false;

            Owner = owningPlayer;
            Owner.Destroyed += HandleDestroyed;

            _interactionTaskCompletionSource = new TaskCompletionSource();
            _interactionTask = Task.Delay(Time, cancellationToken);
        }
        finally
        {
            _semaphore.Release();
        }

        InteractionStarted?.Invoke(this, owningPlayer, Time, cancellationToken);
        try
        {
            var finishedTask = await Task.WhenAny(_interactionTaskCompletionSource.Task, _interactionTask);
            if (finishedTask == _interactionTask)
            {
                if(!cancellationToken.IsCancellationRequested && _interactionTask.IsCompleted)
                    InteractionCompleted?.Invoke(this, owningPlayer, true);
                else
                    InteractionCompleted?.Invoke(this, owningPlayer, false);
                return true;
            }
            InteractionCompleted?.Invoke(this, owningPlayer, false);
            return false;
        }
        finally
        {
            if(Owner != null)
            {
                Owner.Destroyed -= HandleDestroyed;
                Owner = null;
            }
        }
    }

    private void HandleDestroyed(Element element)
    {
        _semaphore.Wait();
        try
        {

            if (_interactionTaskCompletionSource != null)
            {
                _interactionTaskCompletionSource.SetCanceled();
                _interactionTaskCompletionSource = null;
                _interactionTask = null;
            }
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public bool EndInteraction(RealmPlayer owningPlayer)
    {
        if (owningPlayer == null)
            throw new NullReferenceException(nameof(owningPlayer));

        _semaphore.Wait();

        try
        {
            if (Owner != owningPlayer)
                return false;

            if (Owner != null)
            {
                Owner.Destroyed -= HandleDestroyed;
                Owner = null;
            }

            if (_interactionTaskCompletionSource != null)
            {
                _interactionTaskCompletionSource.SetCanceled();
                _interactionTaskCompletionSource = null;
            }

            return true;
        }
        finally
        {
            _semaphore.Release();
        }
    }
}
