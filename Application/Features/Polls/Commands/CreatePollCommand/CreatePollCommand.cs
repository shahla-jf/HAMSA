using HAMSA.Domain.Entities;
using HAMSA.Domain.Enums;
using HAMSA.Domain.Interfaces.Repositories;

public record CreatePollCommand(
    Guid BuildingId,
    Guid UserId,
    string Title,
    string? Description,
    PollAudience Audience,
    DateTime Deadline,
    List<string> Options);

public record CreatePollResult(
    bool Success,
    string Message,
    Guid? PollId = null);
    
    
public class CreatePollHandler
{
    private readonly IPollRepository _pollRepository;
    private readonly IBuildingMembershipRepository _membershipRepository;

    public CreatePollHandler(
        IPollRepository pollRepository,
        IBuildingMembershipRepository membershipRepository)
    {
        _pollRepository = pollRepository;
        _membershipRepository = membershipRepository;
    }

    public async Task<CreatePollResult> HandleAsync(CreatePollCommand command)
    {
        var manager = await _membershipRepository
            .GetActiveAsync(command.UserId, command.BuildingId);

        var buildingManagerId = await _membershipRepository.GetCurrentManagerIdAsync(command.BuildingId);
        
        if (buildingManagerId != command.UserId)
            return new CreatePollResult(false, "شما مدیر این ساختمان نیستید");
        
        if (command.Options.Count < 2)
            return new CreatePollResult(false, "حداقل دو گزینه لازم است.");

        var poll = Poll.Create(
            command.BuildingId,
            command.UserId,
            command.Title,
            command.Description,
            command.Audience,
            command.Deadline);

        foreach (var option in command.Options.Distinct())
        {
            poll.AddOption(option);
        }

        await _pollRepository.AddAsync(poll);
        await _pollRepository.SaveChangesAsync();

        return new CreatePollResult(true, "رأی‌گیری ایجاد شد.", poll.Id);
    }
}