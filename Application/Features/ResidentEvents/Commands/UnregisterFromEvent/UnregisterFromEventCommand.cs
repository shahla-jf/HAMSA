using HAMSA.Domain.Interfaces.Repositories;

namespace HAMSA.Application.Features.ResidentEvents.Commands.UnregisterFromEvent;

public record UnregisterFromEventCommand(Guid EventId, Guid UserId);

public record UnregisterFromEventResult(bool Success, string Message);

public class UnregisterFromEventHandler
{
    private readonly IResidentEventRepository _eventRepository;

    public UnregisterFromEventHandler(IResidentEventRepository eventRepository)
    {
        _eventRepository = eventRepository;
    }

    public async Task<UnregisterFromEventResult> HandleAsync(UnregisterFromEventCommand command)
    {
        var residentEvent = await _eventRepository.GetWithSessionsAsync(command.EventId);

        if (residentEvent is null)
            return new(false, "رویداد یافت نشد.");

        var isRegistered = await _eventRepository.IsRegisteredAsync(command.EventId, command.UserId);
        if (!isRegistered)
            return new(false, "شما در این رویداد ثبت‌نام نکرده‌اید.");

        residentEvent.RemoveRegistration(command.UserId);
        await _eventRepository.SaveChangesAsync();

        return new(true, "ثبت‌نام شما لغو شد.");
    }
}