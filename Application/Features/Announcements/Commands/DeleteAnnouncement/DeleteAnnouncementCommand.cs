using HAMSA.Domain.Interfaces.Repositories;

namespace HAMSA.Application.Features.Announcements.Commands.DeleteAnnouncement;

public record DeleteAnnouncementCommand(
    Guid AnnouncementId,
    Guid UserId);

public record DeleteAnnouncementResult(
    bool Success,
    string Message);

public class DeleteAnnouncementHandler
{
    private readonly IAnnouncementRepository _announcementRepository;
    private readonly IBuildingMembershipRepository _membershipRepository;

    public DeleteAnnouncementHandler(
        IAnnouncementRepository announcementRepository,
        IBuildingMembershipRepository membershipRepository)
    {
        _announcementRepository = announcementRepository;
        _membershipRepository = membershipRepository;
    }

    public async Task<DeleteAnnouncementResult> HandleAsync(
        DeleteAnnouncementCommand command)
    {
        var announcement =
            await _announcementRepository.GetByIdAsync(command.AnnouncementId);

        if (announcement is null)
            return new(false, "اطلاعیه پیدا نشد.");

        var managerId =
            await _membershipRepository.GetCurrentManagerIdAsync(
                announcement.BuildingId);

        if (managerId != command.UserId)
            return new(false, "فقط مدیر ساختمان می‌تواند اطلاعیه را حذف کند.");

        _announcementRepository.Delete(announcement);
        await _announcementRepository.SaveChangesAsync();

        return new(true, "اطلاعیه حذف شد.");
    }
}