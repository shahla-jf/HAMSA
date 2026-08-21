using HAMSA.Domain.Interfaces.Repositories;

namespace HAMSA.Application.Features.GroupBuying.Queries;


/// <summary>
/// لیست خریدهای گروهی ایجادشده توسط من (با لیست شرکت‌کنندگان)
/// </summary>

public record GetMyGroupBuyingsQuery(Guid UserId, Guid BuildingId);

public record GroupBuyingParticipantDto(
    string FullName,
    string ContactPhone);

public record MyGroupBuyingDto(
    Guid Id,
    string Title,
    int MinimumQuantity,
    decimal Price,
    DateTime Deadline,
    int ParticipantCount,
    bool IsDeadlineExpired,
    List<GroupBuyingParticipantDto> Participants);

public record GetMyGroupBuyingsResult(
    bool Success,
    string Message,
    List<MyGroupBuyingDto> Items);

public class GetMyGroupBuyingsHandler
{
    private readonly IGroupBuyingRepository _groupBuyingRepository;

    public GetMyGroupBuyingsHandler(IGroupBuyingRepository groupBuyingRepository)
    {
        _groupBuyingRepository = groupBuyingRepository;
    }

    public async Task<GetMyGroupBuyingsResult> HandleAsync(GetMyGroupBuyingsQuery query)
    {
        var groupBuyings = await _groupBuyingRepository.GetByCreatorInBuildingAsync(query.UserId, query.BuildingId);

        var items = groupBuyings.Select(g => new MyGroupBuyingDto(
            g.Id,
            g.Title,
            g.MinimumQuantity,
            g.Price,
            g.Deadline,
            g.Participants.Count,
            g.IsDeadlineExpired,
            g.Participants.Select(p => new GroupBuyingParticipantDto(
                $"{p.User?.FirstName} {p.User?.LastName}",
                p.User?.PhoneNumber ?? g.ContactPhone
            )).ToList()
        )).ToList();

        return new(true, "لیست خریدهای گروهی شما دریافت شد.", items);
    }
}



/// <summary>
/// لیست کل خریدهای گروهی ساختمان
/// </summary>

public record GetBuildingGroupBuyingsQuery(
    Guid BuildingId,
    Guid UserId);

public record BuildingGroupBuyingDto(
    Guid Id,
    string Title,
    string OrganizerFullName,
    int JoinedUnitsCount,
    decimal Price,
    DateTime Deadline,
    bool IsDeadlineExpired,
    bool HasJoined);

public record GetBuildingGroupBuyingsResult(
    bool Success,
    string Message,
    List<BuildingGroupBuyingDto> Items);

public class GetBuildingGroupBuyingsHandler
{
    private readonly IGroupBuyingRepository _groupBuyingRepository;
    private readonly IBuildingMembershipRepository _membershipRepository;

    public GetBuildingGroupBuyingsHandler(
        IGroupBuyingRepository groupBuyingRepository,
        IBuildingMembershipRepository membershipRepository)
    {
        _groupBuyingRepository = groupBuyingRepository;
        _membershipRepository = membershipRepository;
    }

    public async Task<GetBuildingGroupBuyingsResult> HandleAsync(GetBuildingGroupBuyingsQuery query)
    {
        // بررسی عضویت کاربر در ساختمان
        var membership = await _membershipRepository.GetActiveAsync(query.UserId, query.BuildingId);
        if (membership is null)
            return new(false, "شما عضو این ساختمان نیستید.", new());

        var groupBuyings = await _groupBuyingRepository.GetActiveByBuildingIdAsync(query.BuildingId);

        var items = groupBuyings.Select(g => new BuildingGroupBuyingDto(
            g.Id,
            g.Title,
            g.OrganizerFullName,
            g.Participants.Count,
            g.Price,
            g.Deadline,
            g.IsDeadlineExpired,
            g.Participants.Any(p => p.UserId == query.UserId)
        )).ToList();

        return new(true, "لیست خریدهای گروهی ساختمان دریافت شد.", items);
    }
}



/// <summary>
/// لیست کمپین‌هایی که به آن‌ها پیوسته‌ام
/// </summary>

public record GetJoinedGroupBuyingsQuery(Guid UserId, Guid BuildingId);

public record JoinedGroupBuyingDto(
    Guid Id,
    string Title,
    int ParticipantCount,
    decimal Price,
    DateTime Deadline,
    string OrganizerFullName,
    bool IsDeadlineExpired,
    bool CanLeave);

public record GetJoinedGroupBuyingsResult(
    bool Success,
    string Message,
    List<JoinedGroupBuyingDto> Items);

public class GetJoinedGroupBuyingsHandler
{
    private readonly IGroupBuyingRepository _groupBuyingRepository;

    public GetJoinedGroupBuyingsHandler(IGroupBuyingRepository groupBuyingRepository)
    {
        _groupBuyingRepository = groupBuyingRepository;
    }

    public async Task<GetJoinedGroupBuyingsResult> HandleAsync(GetJoinedGroupBuyingsQuery query)
    {
        var groupBuyings = await _groupBuyingRepository.GetJoinedByUserInBuildingAsync(query.UserId, query.BuildingId);

        var items = groupBuyings.Select(g => new JoinedGroupBuyingDto(
            g.Id,
            g.Title,
            g.Participants.Count,
            g.Price,
            g.Deadline,
            g.OrganizerFullName,
            g.IsDeadlineExpired,
            CanLeave: !g.IsDeadlineExpired
        )).ToList();

        return new(true, "لیست کمپین‌های پیوسته‌شده دریافت شد.", items);
    }
}