using HAMSA.Domain.Enums;
using HAMSA.Domain.Interfaces.Repositories;

namespace HAMSA.Application.Features.Listings.Queries;

/// <summary>
///لیست آگهی ها
/// </summary>
public record GetListingsQuery(Guid BuildingId, Guid UserId);

public record ListingSummaryDto(
    Guid Id,
    string Title,
    string Description,
    DateTime CreatedAt);

public class GetListingsHandler
{
    private readonly IListingRepository _listingRepository;
    private readonly IBuildingMembershipRepository _membershipRepository;

    public GetListingsHandler(
        IListingRepository listingRepository,
        IBuildingMembershipRepository membershipRepository)
    {
        _listingRepository = listingRepository;
        _membershipRepository = membershipRepository;
    }

    public async Task<IEnumerable<ListingSummaryDto>> HandleAsync(GetListingsQuery query)
    {
        var membership = await _membershipRepository.GetActiveAsync(query.UserId, query.BuildingId);
        if (membership is null)
            return Enumerable.Empty<ListingSummaryDto>();

        var listings = await _listingRepository.GetByBuildingIdAsync(query.BuildingId);

        return listings.Select(l => new ListingSummaryDto(
            l.Id,
            l.Title,
            l.Description,
            l.CreatedAt
        ));
    }
}



/// <summary>
/// جزئیات آگهی
/// </summary>
public record GetListingDetailsQuery(Guid ListingId, Guid UserId);

public record ListingDetailDto(
    Guid Id,
    string Title,
    string Description,
    ListingType Type,
    decimal? Price,
    string ContactPhone,
    string? ImageUrl,
    bool IsMine);

public class GetListingDetailsHandler
{
    private readonly IListingRepository _listingRepository;
    private readonly IBuildingMembershipRepository _membershipRepository;

    public GetListingDetailsHandler(
        IListingRepository listingRepository,
        IBuildingMembershipRepository membershipRepository)
    {
        _listingRepository = listingRepository;
        _membershipRepository = membershipRepository;
    }

    public async Task<ListingDetailDto?> HandleAsync(GetListingDetailsQuery query)
    {
        var listing = await _listingRepository.GetByIdAsync(query.ListingId);
        if (listing is null)
            return null;

        // بررسی اینکه کاربر عضو ساختمانی باشد که آگهی متعلق به آن است
        var membership = await _membershipRepository.GetActiveAsync(query.UserId, listing.BuildingId);
        if (membership is null)
            return null;

        return new ListingDetailDto(
            listing.Id,
            listing.Title,
            listing.Description,
            listing.Type,
            listing.Price,
            listing.ContactPhone,
            listing.ImageUrl,
            listing.CreatedByUserId == query.UserId
        );
    }
}



/// <summary>
/// آگهی های من
/// </summary>
public record GetMyListingsQuery(Guid BuildingId, Guid UserId);

public class GetMyListingsHandler
{
    private readonly IListingRepository _listingRepository;
    private readonly IBuildingMembershipRepository _membershipRepository;

    public GetMyListingsHandler(
        IListingRepository listingRepository,
        IBuildingMembershipRepository membershipRepository)
    {
        _listingRepository = listingRepository;
        _membershipRepository = membershipRepository;
    }

    public async Task<IEnumerable<ListingSummaryDto>> HandleAsync(GetMyListingsQuery query)
    {
        var membership = await _membershipRepository.GetActiveAsync(query.UserId, query.BuildingId);
        if (membership is null)
            return Enumerable.Empty<ListingSummaryDto>();

        var listings = await _listingRepository.GetByBuildingIdAndUserIdAsync(query.BuildingId, query.UserId);

        return listings.Select(l => new ListingSummaryDto(
            l.Id,
            l.Title,
            l.Description,
            l.CreatedAt
        ));
    }
}