using HAMSA.Domain.Entities;
using HAMSA.Domain.Enums;
using HAMSA.Domain.Interfaces.Repositories;

namespace HAMSA.Application.Features.GroupChallenge.Commands.MarkChallengeCompleted;

public record MarkChallengeCompletedCommand(Guid BuildingId, Guid UserId);

public record MarkChallengeCompletedResult(bool Success, string Message);

public class MarkChallengeCompletedHandler
{
    private readonly IGroupChallengeRepository _challengeRepository;

    public MarkChallengeCompletedHandler(IGroupChallengeRepository challengeRepository)
    {
        _challengeRepository = challengeRepository;
    }

    public async Task<MarkChallengeCompletedResult> HandleAsync(MarkChallengeCompletedCommand command)
    {
        var challenge = await _challengeRepository.GetCurrentAsync(command.BuildingId);

        if (challenge is null)
            return new(false, "چالشی برای این ساختمان وجود ندارد.");

        if (challenge.Status != GroupChallengeStatus.ChallengeActive)
            return new(false, "چالش در حال حاضر فعال نیست.");

        if (DateTime.UtcNow > challenge.ChallengeDeadline)
            return new(false, "مهلت چالش به پایان رسیده است.");

        var participant = challenge.Participants.FirstOrDefault(p => p.UserId == command.UserId);
        if (participant is null)
            return new(false, "شما در این چالش ثبت‌نام نکرده‌اید.");

        if (participant.IsCompleted)
            return new(false, "شما قبلاً این چالش را انجام داده‌اید.");

        participant.MarkAsCompleted();
        await _challengeRepository.SaveChangesAsync();

        return new(true, "چالش با موفقیت به عنوان انجام‌شده ثبت شد.");
    }
}