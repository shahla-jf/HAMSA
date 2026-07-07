using HAMSA.Domain.Enums;
using HAMSA.Domain.Interfaces.Repositories;

namespace HAMSA.Application.Features.Announcements.Queries;

// ======================================================
// GetAnnouncements
// ======================================================

public record GetAnnouncementsQuery(Guid BuildingId, Guid UserId);

public record AnnouncementDto(
    Guid Id,
    string Title,
    string Description,
    AnnouncementPriority Priority,
    AnnouncementSource Source,
    DateTime CreatedAt,
    bool IsRead);

public class GetAnnouncementsHandler
{
    private readonly IAnnouncementRepository _announcementRepository;
    private readonly IBuildingMembershipRepository _membershipRepository;

    public GetAnnouncementsHandler(
        IAnnouncementRepository announcementRepository,
        IBuildingMembershipRepository membershipRepository)
    {
        _announcementRepository = announcementRepository;
        _membershipRepository = membershipRepository;
    }

    public async Task<IEnumerable<AnnouncementDto>> HandleAsync(GetAnnouncementsQuery query)
    {
        var membership = await _membershipRepository.GetActiveAsync(
            query.UserId,
            query.BuildingId);

        if (membership is null)
            return Enumerable.Empty<AnnouncementDto>();

        var announcements =
            await _announcementRepository.GetByBuildingIdAsync(query.BuildingId);

        return announcements.Select(a => new AnnouncementDto(
            a.Id,
            a.Title,
            a.Description,
            a.Priority,
            a.Source,
            a.CreatedAt,
            a.ReadRecords.Any(r => r.UserId == query.UserId)
        ));
    }
}