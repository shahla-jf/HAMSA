using HAMSA.Domain.Entities;
using HAMSA.Domain.Enums;
using HAMSA.Domain.Interfaces.Repositories;

namespace HAMSA.Application.Features.Announcements.Commands.CreateAnnouncement;

public record CreateAnnouncementCommand(
    Guid BuildingId,
    Guid UserId,
    string Title,
    string Description,
    AnnouncementPriority Priority);

public record CreateAnnouncementResult(
    bool Success,
    string Message,
    Guid? AnnouncementId = null);
    
    
public class CreateAnnouncementHandler
{
    private readonly IAnnouncementRepository _announcementRepository;
    private readonly IBuildingMembershipRepository _membershipRepository;

    public CreateAnnouncementHandler(
        IAnnouncementRepository announcementRepository,
        IBuildingMembershipRepository membershipRepository)
    {
        _announcementRepository = announcementRepository;
        _membershipRepository = membershipRepository;
    }

    public async Task<CreateAnnouncementResult> HandleAsync(CreateAnnouncementCommand command)
    {
        var managerId =
            await _membershipRepository.GetCurrentManagerIdAsync(command.BuildingId);

        if (managerId != command.UserId)
            return new(false, "فقط مدیر ساختمان می‌تواند اطلاعیه ثبت کند.");

        var announcement = Announcement.CreateByManager(
            command.BuildingId,
            command.UserId,
            command.Title,
            command.Description,
            command.Priority);

        await _announcementRepository.AddAsync(announcement);
        await _announcementRepository.SaveChangesAsync();

        return new(
            true,
            "اطلاعیه با موفقیت ثبت شد.",
            announcement.Id);
    }
}