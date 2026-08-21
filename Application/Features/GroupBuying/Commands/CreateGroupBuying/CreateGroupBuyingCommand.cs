using HAMSA.Domain.Interfaces.Repositories;

namespace HAMSA.Application.Features.GroupBuying.Commands.CreateGroupBuying;

public record CreateGroupBuyingCommand(
    Guid BuildingId,
    Guid UserId,
    string OrganizerFullName,
    string Title,
    int MinimumQuantity,
    decimal Price,
    DateTime Deadline,
    string? ImageUrl,
    int Block,
    int Floor,
    int UnitNumber,
    string ContactPhone);

public record CreateGroupBuyingResult(bool Success, string Message, Guid? GroupBuyingId = null);

public class CreateGroupBuyingHandler
{
    private readonly IGroupBuyingRepository _groupBuyingRepository;
    private readonly IBuildingMembershipRepository _membershipRepository;

    public CreateGroupBuyingHandler(
        IGroupBuyingRepository groupBuyingRepository,
        IBuildingMembershipRepository membershipRepository)
    {
        _groupBuyingRepository = groupBuyingRepository;
        _membershipRepository = membershipRepository;
    }

    public async Task<CreateGroupBuyingResult> HandleAsync(CreateGroupBuyingCommand command)
    {
        var membership = await _membershipRepository.GetActiveAsync(command.UserId, command.BuildingId);

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

        var groupBuying = HAMSA.Domain.Entities.GroupBuying.Create(
            command.BuildingId,
            command.UserId,
            command.OrganizerFullName,
            command.Title,
            command.MinimumQuantity,
            command.Price,
            command.Deadline,
            command.ImageUrl,
            command.Block,
            command.Floor,
            command.UnitNumber,
            command.ContactPhone);

        await _groupBuyingRepository.AddAsync(groupBuying);
        await _groupBuyingRepository.SaveChangesAsync();

        return new(true, "خرید گروهی با موفقیت ثبت شد.", groupBuying.Id);
    }
}