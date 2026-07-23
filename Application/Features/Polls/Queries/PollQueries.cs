using HAMSA.Domain.Enums;
using HAMSA.Domain.Interfaces.Repositories;

public record PollOptionDto(
    Guid Id,
    string Text,
    int VoteCount);

public record PollDto(
    Guid Id,
    string Title,
    string? Description,
    DateTime Deadline,
    List<PollOptionDto> Options);
    
    
public class GetActivePollsHandler
{
    private readonly IPollRepository _pollRepository;
    private readonly IBuildingMembershipRepository _membershipRepository;

    public GetActivePollsHandler(
        IPollRepository pollRepository,
        IBuildingMembershipRepository membershipRepository)
    {
        _pollRepository = pollRepository;
        _membershipRepository = membershipRepository;
    }

    public async Task<IEnumerable<PollDto>> HandleAsync(Guid buildingId, Guid userId)
    {
        var membership = await _membershipRepository
            .GetActiveAsync(userId, buildingId);

        if (membership is null)
            return Enumerable.Empty<PollDto>();

        var polls = await _pollRepository
            .GetActiveByBuildingIdAsync(buildingId);

        polls = polls.Where(p =>
            p.Audience == PollAudience.All ||
            (p.Audience == PollAudience.Owners &&
             (membership.Role == UserRole.Owner || membership.Role == UserRole.Manager)) ||
            (p.Audience == PollAudience.Tenants &&
             (membership.Role == UserRole.Tenant || membership.Role == UserRole.Manager)));

        return polls.Select(p => new PollDto(
            p.Id,
            p.Title,
            p.Description,
            p.Deadline,
            p.Options.Select(o => new PollOptionDto(
                    o.Id,
                    o.Text,
                    o.Votes.Count))
                .ToList()));
    }
}


public class GetInActivePollsHandler
{
    private readonly IPollRepository _pollRepository;
    private readonly IBuildingMembershipRepository _membershipRepository;

    public GetInActivePollsHandler(
        IPollRepository pollRepository,
        IBuildingMembershipRepository membershipRepository)
    {
        _pollRepository = pollRepository;
        _membershipRepository = membershipRepository;
    }

    public async Task<IEnumerable<PollDto>> HandleAsync(Guid buildingId, Guid userId)
    {
        var membership = await _membershipRepository
            .GetActiveAsync(userId, buildingId);

        if (membership is null)
            return Enumerable.Empty<PollDto>();

        var polls = await _pollRepository
            .GetInactiveByBuildingIdAsync(buildingId);

        polls = polls.Where(p =>
            p.Audience == PollAudience.All ||
            (p.Audience == PollAudience.Owners &&
             (membership.Role == UserRole.Owner || membership.Role == UserRole.Manager)) ||
            (p.Audience == PollAudience.Tenants &&
             (membership.Role == UserRole.Tenant || membership.Role == UserRole.Manager)));

        return polls.Select(p => new PollDto(
            p.Id,
            p.Title,
            p.Description,
            p.Deadline,
            p.Options.Select(o => new PollOptionDto(
                    o.Id,
                    o.Text,
                    o.Votes.Count))
                .ToList()));
    }
}