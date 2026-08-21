using HAMSA.Domain.Interfaces.Repositories;

namespace HAMSA.Application.Features.GroupBuyings.Commands.LeaveGroupBuying;

public record LeaveGroupBuyingCommand(
    Guid GroupBuyingId,
    Guid UserId);

public record LeaveGroupBuyingResult(bool Success, string Message);

public class LeaveGroupBuyingHandler
{
    private readonly IGroupBuyingRepository _groupBuyingRepository;

    public LeaveGroupBuyingHandler(IGroupBuyingRepository groupBuyingRepository)
    {
        _groupBuyingRepository = groupBuyingRepository;
    }

    public async Task<LeaveGroupBuyingResult> HandleAsync(LeaveGroupBuyingCommand command)
    {
        var groupBuying = await _groupBuyingRepository.GetByIdWithParticipantsAsync(command.GroupBuyingId);

        if (groupBuying is null)
            return new(false, "خرید گروهی یافت نشد.");

        if (groupBuying.IsDeadlineExpired)
            return new(false, "مهلت ثبت‌نام به پایان رسیده و امکان خروج وجود ندارد.");

        var isParticipant = await _groupBuyingRepository.IsParticipantAsync(command.GroupBuyingId, command.UserId);
        if (!isParticipant)
            return new(false, "شما عضو این کمپین نیستید.");

        groupBuying.RemoveParticipant(command.UserId);

        await _groupBuyingRepository.SaveChangesAsync();

        return new(true, "با موفقیت از کمپین خرید خارج شدید.");
    }
}