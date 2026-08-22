using HAMSA.Domain.Interfaces.Repositories;

namespace HAMSA.Application.Features.GroupBuying.Commands.CreateGroupBuying;

public record CreateGroupBuyingCommand(
    Guid BuildingId,
    Guid UserId,
    string Title,
    int MinimumQuantity,
    decimal Price,
    DateTime Deadline,
    string ContactPhone);

public record CreateGroupBuyingResult(bool Success, string Message, Guid? GroupBuyingId = null);

public class CreateGroupBuyingHandler
{
    private readonly IGroupBuyingRepository _groupBuyingRepository;
    private readonly IBuildingMembershipRepository _membershipRepository;
    private readonly IUserRepository _userRepository;

    public CreateGroupBuyingHandler(
        IGroupBuyingRepository groupBuyingRepository,
        IBuildingMembershipRepository membershipRepository,
        IUserRepository userRepository)
    {
        _groupBuyingRepository = groupBuyingRepository;
        _membershipRepository = membershipRepository;
        _userRepository = userRepository;
    }

    public async Task<CreateGroupBuyingResult> HandleAsync(CreateGroupBuyingCommand command)
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
            return new(false, "عنوان خرید نمی‌تواند خالی باشد.");

        if (command.MinimumQuantity < 2)
            return new(false, "حداقل تعداد خرید باید حداقل ۲ نفر باشد.");

        if (command.Price <= 0)
            return new(false, "قیمت باید بیشتر از صفر باشد.");

        if (command.Deadline <= DateTime.UtcNow)
            return new(false, "مهلت خرید باید در آینده باشد.");
        
        var groupBuying = Domain.Entities.GroupBuying.Create(
            command.BuildingId,
            command.UserId,
            $"{user?.FirstName} {user?.LastName}",
            command.Title,
            command.MinimumQuantity,
            command.Price,
            command.Deadline,
            unit.Unit.Block,
            unit.Unit.Floor,
            unit.Unit.UnitNumber,
            command.ContactPhone);

        await _groupBuyingRepository.AddAsync(groupBuying);
        await _groupBuyingRepository.SaveChangesAsync();

        return new(true, "خرید گروهی با موفقیت ثبت شد.", groupBuying.Id);
    }
}