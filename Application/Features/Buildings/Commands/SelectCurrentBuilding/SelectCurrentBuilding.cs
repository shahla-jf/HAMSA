using HAMSA.Domain.Interfaces.Repositories;

namespace HAMSA.Application.Features.Buildings.Commands.SelectCurrentBuilding;

public record SelectCurrentBuildingCommand(
    Guid UserId,
    Guid BuildingId);

public record SelectCurrentBuildingResult(
    bool Success,
    string Message);
    

public class SelectCurrentBuildingHandler
{
    private readonly IBuildingMembershipRepository _membershipRepository;

    public SelectCurrentBuildingHandler(
        IBuildingMembershipRepository membershipRepository)
    {
        _membershipRepository = membershipRepository;
    }

    public async Task<SelectCurrentBuildingResult> HandleAsync(
        SelectCurrentBuildingCommand command)
    {
        var membership = await _membershipRepository
            .GetActiveAsync(command.UserId, command.BuildingId);

        if (membership is null)
            return new(false, "کاربر عضو این ساختمان نیست.");

        membership.MarkAsLastSelected();

        await _membershipRepository.SaveChangesAsync();

        return new(true, "ساختمان جاری ذخیره شد.");
    }
}