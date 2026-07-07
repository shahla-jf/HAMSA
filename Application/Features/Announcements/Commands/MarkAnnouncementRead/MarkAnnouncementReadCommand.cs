using HAMSA.Domain.Interfaces.Repositories;

namespace HAMSA.Application.Features.Announcements.Commands.MarkAnnouncementRead;

public record MarkAnnouncementReadCommand(
    Guid AnnouncementId,
    Guid UserId);

public record MarkAnnouncementReadResult(
    bool Success,
    string Message);
    

public class MarkAnnouncementReadHandler
{
    private readonly IAnnouncementRepository _announcementRepository;
    private readonly IBuildingMembershipRepository _membershipRepository;

    public MarkAnnouncementReadHandler(
        IAnnouncementRepository announcementRepository,
        IBuildingMembershipRepository membershipRepository)
    {
        _announcementRepository = announcementRepository;
        _membershipRepository = membershipRepository;
    }

    public async Task<MarkAnnouncementReadResult> HandleAsync(
        MarkAnnouncementReadCommand command)
    {
        var announcement =
            await _announcementRepository.GetWithReadsAsync(command.AnnouncementId);

        if (announcement is null)
            return new(false, "اطلاعیه پیدا نشد.");

        var membership = await _membershipRepository.GetActiveAsync(
            command.UserId,
            announcement.BuildingId);

        if (membership is null)
            return new(false, "دسترسی غیرمجاز.");

        var isNewlyRead = announcement.MarkAsRead(command.UserId);

        if (isNewlyRead)
        {
            var newReadRecord = announcement.ReadRecords
                .Single(r => r.UserId == command.UserId);
            _announcementRepository.AddReadRecord(newReadRecord);
        }

        await _announcementRepository.SaveChangesAsync();

        return new(true, "اطلاعیه به عنوان خوانده شده ثبت شد.");
    }
}