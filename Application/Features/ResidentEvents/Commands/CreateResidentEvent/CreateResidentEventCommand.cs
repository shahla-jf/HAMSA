using HAMSA.Domain.Entities;
using HAMSA.Domain.Enums;
using HAMSA.Domain.Interfaces.Repositories;

namespace HAMSA.Application.Features.ResidentEvents.Commands.CreateResidentEvent;

public record CreateResidentEventCommand(
    Guid BuildingId,
    Guid UserId,
    EventCategory Category,
    string Title,
    string Description,
    decimal RegistrationFee,
    string EventTime);

public record CreateResidentEventResult(bool Success, string Message, Guid? EventId = null);

public class CreateResidentEventHandler
{
    private readonly IResidentEventRepository _eventRepository;
    private readonly IBuildingMembershipRepository _membershipRepository;
    private readonly IUserRepository _userRepository;

    public CreateResidentEventHandler(
        IResidentEventRepository eventRepository,
        IBuildingMembershipRepository membershipRepository,
        IUserRepository userRepository)
    {
        _eventRepository = eventRepository;
        _membershipRepository = membershipRepository;
        _userRepository = userRepository;
    }

    public async Task<CreateResidentEventResult> HandleAsync(CreateResidentEventCommand command)
    {
        var membership = await _membershipRepository.GetActiveAsync(command.UserId, command.BuildingId);
        var user = await _userRepository.GetByIdAsync(command.UserId);
        
        var units = await _membershipRepository.GetUserUnitsInBuildingAsync(command.UserId, command.BuildingId);
        var unit = units.FirstOrDefault();

        if (unit is null || unit.Unit is null)
            return new(false, "شما هیچ واحدی در این ساختمان ندارید.");
        
        if (membership is null)
            return new(false, "شما عضو این ساختمان نیستید.");

        if (string.IsNullOrWhiteSpace(command.Title))
            return new(false, "عنوان رویداد نمی‌تواند خالی باشد.");


        var residentEvent = ResidentEvent.Create(
            command.BuildingId,
            command.UserId,
            $"{user.FirstName} {user.LastName}",
            command.EventTime,
            command.Category,
            command.Title,
            command.Description,
            user.PhoneNumber,
            unit.Unit.Block,
            unit.Unit.Floor,
            unit.Unit.UnitNumber,
            command.RegistrationFee);


        await _eventRepository.AddAsync(residentEvent);
        await _eventRepository.SaveChangesAsync();

        return new(true, "رویداد با موفقیت ثبت شد.", residentEvent.Id);
    }
}