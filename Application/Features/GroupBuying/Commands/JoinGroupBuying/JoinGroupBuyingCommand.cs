using HAMSA.Domain.Entities;
using HAMSA.Domain.Interfaces.Repositories;

namespace HAMSA.Application.Features.GroupBuyings.Commands.JoinGroupBuying;

public record JoinGroupBuyingCommand(
    Guid GroupBuyingId,
    Guid UserId);

public record JoinGroupBuyingResult(bool Success, string Message);

public class JoinGroupBuyingHandler
{
    private readonly IGroupBuyingRepository _groupBuyingRepository;
    private readonly IBuildingMembershipRepository _membershipRepository;

    public JoinGroupBuyingHandler(
        IGroupBuyingRepository groupBuyingRepository,
        IBuildingMembershipRepository membershipRepository)
    {
        _groupBuyingRepository = groupBuyingRepository;
        _membershipRepository = membershipRepository;
    }

    public async Task<JoinGroupBuyingResult> HandleAsync(JoinGroupBuyingCommand command)
    {
        var groupBuying = await _groupBuyingRepository.GetByIdWithParticipantsAsync(command.GroupBuyingId);

        if (groupBuying is null)
            return new(false, "خرید گروهی یافت نشد.");

        // بررسی عضویت کاربر در ساختمان
        var membership = await _membershipRepository.GetActiveAsync(command.UserId, groupBuying.BuildingId);
        if (membership is null)
            return new(false, "شما عضو این ساختمان نیستید.");

        // بررسی مهلت
        if (groupBuying.IsDeadlineExpired)
            return new(false, "مهلت ثبت‌نام این کمپین به پایان رسیده است.");

        // بررسی تکراری نبودن
        var isAlreadyParticipant = await _groupBuyingRepository.IsParticipantAsync(command.GroupBuyingId, command.UserId);
        if (isAlreadyParticipant)
            return new(false, "شما قبلاً به این کمپین پیوسته‌اید.");

        if (groupBuying.IsOwner(command.UserId))
            return new(false, "شما ایجادکننده این کمپین هستید و نیازی به پیوستن نیست.");

        groupBuying.AddParticipant(command.UserId);

        await _groupBuyingRepository.SaveChangesAsync();

        return new(true, "با موفقیت به کمپین خرید پیوستید.");
    }
}