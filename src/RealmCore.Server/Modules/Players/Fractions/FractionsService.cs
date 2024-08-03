namespace RealmCore.Server.Modules.Players.Fractions;

public sealed class FractionsService
{
    private readonly Dictionary<int, FractionDto> _fractions = [];
    private readonly SemaphoreSlim _semaphoreSlim = new(1);
    private readonly IFractionRepository _fractionRepository;
    private readonly IServiceScope _serviceScope;
    private readonly UsersInUse _usersInUse;

    public FractionsService(IServiceProvider serviceProvider, UsersInUse usersInUse)
    {
        _serviceScope = serviceProvider.CreateScope();
        _fractionRepository = _serviceScope.ServiceProvider.GetRequiredService<IFractionRepository>();
        _usersInUse = usersInUse;
    }

    public async Task LoadFractions(CancellationToken cancellationToken = default)
    {
        await _semaphoreSlim.WaitAsync(cancellationToken);
        try
        {
            var fractionDataList = await _fractionRepository.GetAll(cancellationToken);
            foreach (var fractionData in fractionDataList)
            {
                var fractionDto = FractionDto.Map(fractionData);
                _fractions[fractionData.Id] = fractionDto;
            }
        }
        finally
        {
            _semaphoreSlim.Release();
        }
    }

    public bool Exists(int fractionId)
    {
        _semaphoreSlim.Wait();
        try
        {
            return _fractions.ContainsKey(fractionId);
        }
        finally
        {
            _semaphoreSlim.Release();
        }
    }

    private bool InternalHasMember(int fractionId, int userId)
    {
        if (!_fractions.ContainsKey(fractionId))
            return false;

        return _fractions[fractionId].Members.Any(x => x.FractionId == userId);
    }

    public bool HasMember(int fractionId, int userId)
    {
        _semaphoreSlim.Wait();
        try
        {
            return InternalHasMember(fractionId, userId);
        }
        finally
        {
            _semaphoreSlim.Release();
        }
    }

    public async Task AddMember(int fractionId, int userId, int rank, string rankName, CancellationToken cancellationToken = default)
    {
        if (_usersInUse.TryGetPlayerByUserId(userId, out var player) && player != null)
        {
            await AddMember(fractionId, player, rank, rankName, cancellationToken);
            return;
        }

        await _semaphoreSlim.WaitAsync(cancellationToken);
        try
        {
            var memberData = await _fractionRepository.TryAddMember(fractionId, userId, rank, rankName, cancellationToken);
            if (memberData == null)
                throw new FractionMemberAlreadyAddedException(userId, fractionId);

            _fractions[fractionId].Members.Add(FractionMemberDto.Map(memberData));
        }
        finally
        {
            _semaphoreSlim.Release();
        }
    }

    public async Task AddMember(int fractionId, RealmPlayer player, int rank, string rankName, CancellationToken cancellationToken = default)
    {
        await _semaphoreSlim.WaitAsync(cancellationToken);
        try
        {
            var memberData = await _fractionRepository.TryAddMember(fractionId, player.UserId, rank, rankName, cancellationToken);
            if (memberData == null)
                throw new FractionMemberAlreadyAddedException(player.UserId, fractionId);
            player.Fractions.AddMember(memberData);

            _fractions[fractionId].Members.Add(FractionMemberDto.Map(memberData));
        }
        finally
        {
            _semaphoreSlim.Release();
        }
    }

    public async Task<bool> TryCreateFraction(int fractionId, string fractionName, string fractionCode, Vector3 position, CancellationToken cancellationToken = default)
    {
        await _semaphoreSlim.WaitAsync(cancellationToken);
        try
        {
            var fractionData = await _fractionRepository.CreateOrGet(fractionId, fractionName, fractionCode, cancellationToken);
            if (fractionData == null)
                return false;

            var fractionDto = FractionDto.Map(fractionData);
            _fractions[fractionData.Id] = fractionDto;
            return true;
        }
        finally
        {
            _semaphoreSlim.Release();
        }
    }
}
