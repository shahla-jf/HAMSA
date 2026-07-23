using HAMSA.Domain.Entities;
using HAMSA.Domain.Enums;
using HAMSA.Domain.Interfaces.Repositories;

namespace HAMSA.Application.Features.Polls.Commands.VotePoll;

public record VotePollCommand(
    Guid PollId,
    Guid OptionId,
    Guid UserId);
    
public record VotePollResult(
    bool Success,
    string Message);
   

public class VotePollHandler
{
    private readonly IPollRepository _pollRepository;
    private readonly IBuildingMembershipRepository _membershipRepository;

    public VotePollHandler(IPollRepository pollRepository,
        IBuildingMembershipRepository membershipRepository)
    {
        _pollRepository = pollRepository;
        _membershipRepository = membershipRepository;
    }

    public async Task<VotePollResult> HandleAsync(VotePollCommand command)
    {
        var poll = await _pollRepository
            .GetWithOptionsAndVotesAsync(command.PollId);

        if (poll is null)
            return new(false, "رأی‌گیری یافت نشد.");

        var membership = await _membershipRepository
            .GetActiveAsync(command.UserId, poll.BuildingId);

        if (membership is null)
            return new(false, "شما عضو این ساختمان نیستید.");
        
        switch (poll.Audience)
        {
            case PollAudience.All:
                break;

            case PollAudience.Owners
                when membership.Role != UserRole.Owner:
                return new(false, "این رأی‌گیری فقط برای مالکین است.");

            case PollAudience.Tenants
                when membership.Role != UserRole.Tenant:
                return new(false, "این رأی‌گیری فقط برای مستاجرین است.");
        }
        
        if (!poll.IsActive)
            return new(false, "مهلت رأی‌گیری پایان یافته است.");

        var option = poll.Options
            .FirstOrDefault(x => x.Id == command.OptionId);

        if (option is null)
            return new(false, "گزینه نامعتبر است.");

        var oldVote = await _pollRepository
            .GetUserVoteAsync(command.PollId, command.UserId);

        if (oldVote != null)
            _pollRepository.RemoveVote(oldVote);

        await _pollRepository.AddVoteAsync(
            PollVote.Create(command.OptionId, command.UserId));

        await _pollRepository.SaveChangesAsync();

        return new(true, "رأی ثبت شد.");
    }
}