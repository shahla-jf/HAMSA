using HAMSA.Domain.Interfaces.Repositories;

namespace HAMSA.Application.Features.GroupBuying.Commands.DeleteGroupBuying;

public record DeleteGroupBuyingCommand(
    Guid GroupBuyingId,
    Guid UserId);

public record DeleteGroupBuyingResult(bool Success, string Message);

public class DeleteGroupBuyingHandler
{
    private readonly IGroupBuyingRepository _groupBuyingRepository;

    public DeleteGroupBuyingHandler(IGroupBuyingRepository groupBuyingRepository)
    {
        _groupBuyingRepository = groupBuyingRepository;
    }

    public async Task<DeleteGroupBuyingResult> HandleAsync(DeleteGroupBuyingCommand command)
    {
        var groupBuying = await _groupBuyingRepository.GetByIdWithParticipantsAsync(command.GroupBuyingId);

        if (groupBuying is null)
            return new(false, "خرید گروهی یافت نشد.");

        if (!groupBuying.IsOwner(command.UserId))
            return new(false, "شما فقط می‌توانید خریدهای گروهی خودتان را حذف کنید.");

        await _groupBuyingRepository.RemoveAsync(groupBuying);
        await _groupBuyingRepository.SaveChangesAsync();

        return new(true, "خرید گروهی با موفقیت حذف شد.");
    }
}