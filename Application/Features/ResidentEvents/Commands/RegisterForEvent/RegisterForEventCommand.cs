using HAMSA.Domain.Interfaces.Repositories;

namespace HAMSA.Application.Features.ResidentEvents.Commands.RegisterForEvent;

public record RegisterForEventCommand(Guid EventId, Guid UserId);

public record RegisterForEventResult(bool Success, string Message);

public class RegisterForEventHandler
{
    private readonly IResidentEventRepository _eventRepository;
    private readonly IBuildingMembershipRepository _membershipRepository;

    public RegisterForEventHandler(
        IResidentEventRepository eventRepository,
        IBuildingMembershipRepository membershipRepository)
    {
        _eventRepository = eventRepository;
        _membershipRepository = membershipRepository;
    }

    public async Task<RegisterForEventResult> HandleAsync(RegisterForEventCommand command)
    {
        var residentEvent = await _eventRepository.GetWithSessionsAsync(command.EventId);

        if (residentEvent is null)
            return new(false, "رویداد یافت نشد.");

        var membership = await _membershipRepository.GetActiveAsync(command.UserId, residentEvent.BuildingId);
        if (membership is null)
            return new(false, "شما عضو این ساختمان نیستید.");

        var isAlreadyRegistered = await _eventRepository.IsRegisteredAsync(command.EventId, command.UserId);
        if (isAlreadyRegistered)
            return new(false, "شما قبلاً در این رویداد ثبت‌نام کرده‌اید.");

        if (residentEvent.IsOrganizer(command.UserId))
            return new(false, "شما برگزارکننده این رویداد هستید.");

        residentEvent.AddRegistration(command.UserId);
        await _eventRepository.SaveChangesAsync();

        return new(true, "ثبت‌نام شما با موفقیت انجام شد.");
    }
}