using HAMSA.Domain.Enums;
using HAMSA.Domain.Interfaces.Repositories;

namespace HAMSA.Application.Features.ResidentEvents.Queries;

/// <summary>
/// لیست رویدادهای ساختمان
/// </summary>
public record GetBuildingEventsQuery(Guid BuildingId, Guid UserId);

public record BuildingEventDto(
    Guid Id,
    string OrganizerFullName,
    EventCategory Category,
    string CategoryName,
    string Title);

public record GetBuildingEventsResult(
    bool Success,
    string Message,
    List<BuildingEventDto> Items);

public class GetBuildingEventsHandler
{
    private readonly IResidentEventRepository _eventRepository;
    private readonly IBuildingMembershipRepository _membershipRepository;

    public GetBuildingEventsHandler(
        IResidentEventRepository eventRepository,
        IBuildingMembershipRepository membershipRepository)
    {
        _eventRepository = eventRepository;
        _membershipRepository = membershipRepository;
    }

    public async Task<GetBuildingEventsResult> HandleAsync(GetBuildingEventsQuery query)
    {
        var membership = await _membershipRepository.GetActiveAsync(query.UserId, query.BuildingId);
        if (membership is null)
            return new(false, "شما عضو این ساختمان نیستید.", new());

        var events = await _eventRepository.GetByBuildingIdAsync(query.BuildingId);

        var items = events.Select(e => new BuildingEventDto(
            e.Id,
            e.OrganizerFullName,
            e.Category,
            GetCategoryDisplayName(e.Category),
            e.Title
        )).ToList();

        return new(true, "لیست رویدادها دریافت شد.", items);
    }

    private string GetCategoryDisplayName(EventCategory category) => category switch
    {
        EventCategory.Cooking => "آشپزی",
        EventCategory.Educational => "آموزشی",
        EventCategory.Beauty => "زیبایی",
        EventCategory.Technical => "فنی",
        EventCategory.Sports => "ورزشی",
        EventCategory.BookReading => "کتابخوانی",
        EventCategory.Parents => "والدین",
        _ => "نامشخص"
    };
}



/// <summary>
/// جزئیات رویداد
/// </summary>
public record GetEventDetailsQuery(Guid EventId, Guid UserId);

public record EventDetails(
    bool Success,
    string Message,
    Guid Id,
    string OrganizerFullName,
    EventCategory Category,
    string CategoryName,
    string Title,
    string Description,
    string EventTime,
    string ContactPhone,
    string Location,
    decimal RegistrationFee,
    bool IsRegistered);

public class GetEventDetailsHandler
{
    private readonly IResidentEventRepository _eventRepository;
    private readonly IBuildingMembershipRepository _membershipRepository;

    public GetEventDetailsHandler(
        IResidentEventRepository eventRepository,
        IBuildingMembershipRepository membershipRepository)
    {
        _eventRepository = eventRepository;
        _membershipRepository = membershipRepository;
    }

    public async Task<EventDetails> HandleAsync(GetEventDetailsQuery query)
    {
        var residentEvent = await _eventRepository.GetWithSessionsAsync(query.EventId);

        if (residentEvent is null)
            return new(false, "رویداد یافت نشد.", Guid.Empty, "", default, "", "", "", "", "", "", 0, false);

        var membership = await _membershipRepository.GetActiveAsync(query.UserId, residentEvent.BuildingId);
        if (membership is null)
            return new(false, "شما به این رویداد دسترسی ندارید.", Guid.Empty, "", default, "", "", "", "", "", "", 0, false);

        var isRegistered = await _eventRepository.IsRegisteredAsync(query.EventId, query.UserId);

        var location = $"بلوک {residentEvent.Block}، طبقه {residentEvent.Floor}، واحد {residentEvent.UnitNumber}";

        return new(
            true,
            "جزئیات رویداد دریافت شد.",
            residentEvent.Id,
            residentEvent.OrganizerFullName,
            residentEvent.Category,
            GetCategoryDisplayName(residentEvent.Category),
            residentEvent.Title,
            residentEvent.Description,
            residentEvent.EventTime,
            residentEvent.ContactPhone,
            location,
            residentEvent.RegistrationFee,
            isRegistered
        );
    }

    private string GetCategoryDisplayName(EventCategory category) => category switch
    {
        EventCategory.Cooking => "آشپزی",
        EventCategory.Educational => "آموزشی",
        EventCategory.Beauty => "زیبایی",
        EventCategory.Technical => "فنی",
        EventCategory.Sports => "ورزشی",
        EventCategory.BookReading => "کتابخوانی",
        EventCategory.Parents => "والدین",
        _ => "نامشخص"
    };
}


/// <summary>
/// رویدادهای من (با لیست شرکت‌کنندگان)
/// </summary>
public record GetMyEventsQuery(Guid UserId);

public record EventParticipantDto(string FullName, string PhoneNumber);

public record MyEventDto(
    Guid Id,
    EventCategory Category,
    string CategoryName,
    string Title,
    List<EventParticipantDto> Participants);

public record GetMyEventsResult(bool Success, string Message, List<MyEventDto> Items);

public class GetMyEventsHandler
{
    private readonly IResidentEventRepository _eventRepository;

    public GetMyEventsHandler(IResidentEventRepository eventRepository)
    {
        _eventRepository = eventRepository;
    }

    public async Task<GetMyEventsResult> HandleAsync(GetMyEventsQuery query)
    {
        var events = await _eventRepository.GetByOrganizerAsync(query.UserId);

        var items = events.Select(e => new MyEventDto(
            e.Id,
            e.Category,
            GetCategoryDisplayName(e.Category),
            e.Title,
            e.Registrations.Select(r => new EventParticipantDto(
                $"{r.User.FirstName} {r.User.LastName}",
                r.User?.PhoneNumber ?? ""
            )).ToList()
        )).ToList();

        return new(true, "لیست رویدادهای شما دریافت شد.", items);
    }

    private string GetCategoryDisplayName(EventCategory category) => category switch
    {
        EventCategory.Cooking => "آشپزی",
        EventCategory.Educational => "آموزشی",
        EventCategory.Beauty => "زیبایی",
        EventCategory.Technical => "فنی",
        EventCategory.Sports => "ورزشی",
        EventCategory.BookReading => "کتابخوانی",
        EventCategory.Parents => "والدین",
        _ => "نامشخص"
    };
}



/// <summary>
/// رویدادهای پیوسته شده
/// </summary>
public record GetJoinedEventsQuery(Guid UserId);

public record JoinedEventDto(
    Guid Id,
    string OrganizerFullName,
    EventCategory Category,
    string CategoryName,
    string EventTime,
    string Location);

public record GetJoinedEventsResult(bool Success, string Message, List<JoinedEventDto> Items);

public class GetJoinedEventsHandler
{
    private readonly IResidentEventRepository _eventRepository;

    public GetJoinedEventsHandler(IResidentEventRepository eventRepository)
    {
        _eventRepository = eventRepository;
    }

    public async Task<GetJoinedEventsResult> HandleAsync(GetJoinedEventsQuery query)
    {
        var events = await _eventRepository.GetRegisteredByUserAsync(query.UserId);

        var items = events.Select(e => new JoinedEventDto(
            e.Id,
            e.OrganizerFullName,
            e.Category,
            GetCategoryDisplayName(e.Category),
            e.EventTime,
            $"بلوک {e.Block}، طبقه {e.Floor}، واحد {e.UnitNumber}"
        )).ToList();

        return new(true, "لیست رویدادهای پیوسته‌شده دریافت شد.", items);
    }

    private string GetCategoryDisplayName(EventCategory category) => category switch
    {
        EventCategory.Cooking => "آشپزی",
        EventCategory.Educational => "آموزشی",
        EventCategory.Beauty => "زیبایی",
        EventCategory.Technical => "فنی",
        EventCategory.Sports => "ورزشی",
        EventCategory.BookReading => "کتابخوانی",
        EventCategory.Parents => "والدین",
        _ => "نامشخص"
    };
}