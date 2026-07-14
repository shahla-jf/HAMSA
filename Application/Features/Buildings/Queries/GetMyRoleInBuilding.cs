using HAMSA.Domain.Enums;
using HAMSA.Domain.Interfaces.Repositories;

namespace HAMSA.Application.Features.Buildings.Queries;

public record GetMyRoleInBuildingQuery(
    Guid UserId,
    Guid BuildingId);

public record GetMyRoleInBuildingResult(
    bool Success,
    string Message,
    UserRole? Role);
    


public class GetMyRoleInBuildingHandler
{
    private readonly IBuildingMembershipRepository _membershipRepository;

    public GetMyRoleInBuildingHandler(
        IBuildingMembershipRepository membershipRepository)
    {
        _membershipRepository = membershipRepository;
    }

    public async Task<GetMyRoleInBuildingResult> HandleAsync(
        GetMyRoleInBuildingQuery query)
    {
        var membership = await _membershipRepository
            .GetActiveAsync(query.UserId, query.BuildingId);

        if (membership is null)
        {
            return new(
                false,
                "کاربر عضو این ساختمان نیست.",
                null);
        }

        return new(
            true,
            "عملیات موفق بود.",
            membership.Role);
    }
}