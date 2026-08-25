using HAMSA.Domain.Interfaces.Repositories;

namespace HAMSA.Application.Features.ResidentEvents.Commands.DeleteResidentEvent;

public record DeleteResidentEventCommand(Guid EventId, Guid UserId);

public record DeleteResidentEventResult(bool Success, string Message);

public class DeleteResidentEventHandler
{
    private readonly IResidentEventRepository _eventRepository;

    public DeleteResidentEventHandler(IResidentEventRepository eventRepository)
    {
        _eventRepository = eventRepository;
    }

    public async Task<DeleteResidentEventResult> HandleAsync(DeleteResidentEventCommand command)
    {
        var residentEvent = await _eventRepository.GetWithSessionsAsync(command.EventId);

        if (residentEvent is null)
            return new(false, "رویداد یافت نشد.");

        if (!residentEvent.IsOrganizer(command.UserId))
            return new(false, "شما فقط می‌توانید رویدادهای خودتان را حذف کنید.");

        await _eventRepository.RemoveAsync(residentEvent);
        await _eventRepository.SaveChangesAsync();

        return new(true, "رویداد با موفقیت حذف شد.");
    }
}