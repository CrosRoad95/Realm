using static RealmCore.Server.Modules.Friends.FriendsResults;

namespace RealmCore.Server.Modules.Friends;

public static class FriendsResults
{
    public record struct RequestSent();
    public record struct RequestAccepted();
    public record struct FriendRemoved();
    public record struct RequestAlreadySent();
    public record struct FailedToSendRequest();
    public record struct FailedToAcceptRequest();
    public record struct FailedToRejectRequest();
    public record struct FriendRequestRejected();
    public record struct AlreadyFriends();
    public record struct FailedToRemoveFriend();
}

public record struct FriendsInfo(int[] Friends, int[] PendingIncomingRequests, int[] PendingOutgoingRequests);

public sealed class FriendsService
{
    private readonly IServiceScope _scope;
    private readonly IServiceProvider _serviceProvider;
    private readonly IFriendRepository _friendRepository;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ILogger<FriendsService> _logger;

    public FriendsService(IServiceProvider serviceProvider, IDateTimeProvider dateTimeProvider, ILogger<FriendsService> logger)
    {
        _scope = serviceProvider.CreateScope();
        _serviceProvider = _scope.ServiceProvider;
        _friendRepository = _serviceProvider.GetRequiredService<IFriendRepository>();
        _dateTimeProvider = dateTimeProvider;
        _logger = logger;
    }

    public async Task<FriendsInfo> GetAllFriends(int userId)
    {
        var friends = await _friendRepository.GetByUserId(userId);
        var incoming = await _friendRepository.GetPendingIncomingFriendsRequests(userId);
        var outgoing = await _friendRepository.GetPendingOutgoingFriendsRequests(userId);

        return new FriendsInfo
        {
            Friends = friends,
            PendingIncomingRequests = incoming,
            PendingOutgoingRequests = outgoing,
        };
    }

    public async Task<OneOf<FriendRemoved, FailedToRemoveFriend>> RemoveFriend(int userId1, int userId2)
    {
        using var activity = Activity.StartActivity(nameof(RemoveFriend));
        if (await _friendRepository.AreFriends(userId1, userId2))
        {
            if(await _friendRepository.RemoveFriend(userId1, userId2))
                return new FriendRemoved();
        }

        return new FailedToRemoveFriend();
    }

    public async Task<OneOf<RequestSent, RequestAlreadySent, FailedToSendRequest>> SendFriendRequest(int userId1, int userId2)
    {
        using var activity = Activity.StartActivity(nameof(SendFriendRequest));

        if (userId1 == userId2)
        {
            activity?.SetStatus(ActivityStatusCode.Error, "Failed to send request, users are the same");
            return new FailedToSendRequest();
        }

        if(await _friendRepository.IsPendingFriendRequest(userId1, userId2))
        {
            activity?.SetStatus(ActivityStatusCode.Ok, "Request already sent");
            return new RequestAlreadySent();
        }

        try
        {
            await _friendRepository.CreatePendingRequest(userId1, userId2, _dateTimeProvider.Now);
            activity?.SetStatus(ActivityStatusCode.Ok, "Request sent");
            return new RequestSent();
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Failed to send friend request");
            activity?.SetStatus(ActivityStatusCode.Error, "Failed to send request");
            return new FailedToSendRequest();
        }
    }

    public async Task<OneOf<RequestAccepted, FailedToAcceptRequest, AlreadyFriends>> AcceptFriendRequest(int userId1, int userId2)
    {
        using var activity = Activity.StartActivity(nameof(AcceptFriendRequest));

        if (userId1 == userId2)
        {
            activity?.SetStatus(ActivityStatusCode.Error, "Failed to accept request, users are the same");
            return new FailedToAcceptRequest();
        }

        if (!await _friendRepository.IsPendingFriendRequest(userId1, userId2))
        {
            activity?.SetStatus(ActivityStatusCode.Error, "Failed to accept request, no pending request");
            return new FailedToAcceptRequest();
        }
        
        if (!await _friendRepository.AreFriends(userId1, userId2))
        {
            activity?.SetStatus(ActivityStatusCode.Error, "Failed to accept request, already friends");
            return new AlreadyFriends();
        }

        try
        {
            await _friendRepository.RemovePendingRequest(userId1, userId2);
            await _friendRepository.CreateFriend(userId1, userId2, _dateTimeProvider.Now);
            activity?.SetStatus(ActivityStatusCode.Ok, "Request accepted");
            return new RequestAccepted();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send friend request");
            activity?.SetStatus(ActivityStatusCode.Error, "Failed to send request");
            return new FailedToAcceptRequest();
        }
    }
    
    public async Task<OneOf<FriendRequestRejected, FailedToRejectRequest>> RejectFriendRequest(int userId1, int userId2)
    {
        using var activity = Activity.StartActivity(nameof(RejectFriendRequest));

        if (userId1 == userId2)
        {
            activity?.SetStatus(ActivityStatusCode.Error, "Failed to reject request, users are the same");
            return new FailedToRejectRequest();
        }

        if (!await _friendRepository.IsPendingFriendRequest(userId1, userId2))
        {
            activity?.SetStatus(ActivityStatusCode.Error, "Failed to reject request, no request");
            return new FailedToRejectRequest();
        }

        await _friendRepository.RemovePendingRequest(userId1, userId2);
        activity?.SetStatus(ActivityStatusCode.Ok, "Friend request removed");
        return new FriendRequestRejected();
    }

    public static readonly ActivitySource Activity = new("RealmCore.FriendRepository", "1.0.0");
}
