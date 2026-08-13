using HAMSA.Domain.Entities;
using HAMSA.Domain.Enums;
using HAMSA.Domain.Interfaces.Repositories;

namespace HAMSA.Application.Features.LocalServices.Commands.CreateLocalService;

public record CreateLocalServiceCommand(
    Guid BuildingId,
    Guid UserId,
    LocalServiceCategory Category,
    string Title,
    string Description,
    string ProviderName,
    string ContactPhone,
    string WorkingHours);

public record CreateLocalServiceResult(bool Success, string Message, Guid? ServiceId = null);

public class CreateLocalServiceHandler
{
    private readonly ILocalServiceRepository _serviceRepository;
    private readonly IBuildingMembershipRepository _membershipRepository;

    public CreateLocalServiceHandler(
        ILocalServiceRepository serviceRepository,
        IBuildingMembershipRepository membershipRepository)
    {
        _serviceRepository = serviceRepository;
        _membershipRepository = membershipRepository;
    }

    public async Task<CreateLocalServiceResult> HandleAsync(CreateLocalServiceCommand command)
    {
        var membership = await _membershipRepository.GetActiveAsync(command.UserId, command.BuildingId);
        
        if (membership is null)
            return new(false, "شما عضو این ساختمان نیستید.");

        var service = LocalService.Create(
            command.BuildingId,
            command.UserId,
            command.Category,
            command.Title,
            command.Description,
            command.ProviderName,
            command.ContactPhone,
            command.WorkingHours);

        await _serviceRepository.AddAsync(service);
        await _serviceRepository.SaveChangesAsync();

        return new(true, "خدمت با موفقیت ثبت شد.", service.Id);
    }
}