using HAMSA.Domain.Entities;
using HAMSA.Domain.Enums;
using HAMSA.Domain.Interfaces.Repositories;

namespace HAMSA.Application.Features.Listings.Commands.CreateListing;

public record CreateListingCommand(
    Guid BuildingId,
    Guid UserId,
    string Title,
    string Description,
    ListingType Type,
    decimal? Price,
    string ContactPhone,
    string? ImageUrl);

public record CreateListingResult(
    bool Success,
    string Message,
    Guid? ListingId = null);

public class CreateListingHandler
{
    private readonly IListingRepository _listingRepository;
    private readonly IBuildingMembershipRepository _membershipRepository;

    public CreateListingHandler(
        IListingRepository listingRepository,
        IBuildingMembershipRepository membershipRepository)
    {
        _listingRepository = listingRepository;
        _membershipRepository = membershipRepository;
    }

    public async Task<CreateListingResult> HandleAsync(CreateListingCommand command)
    {
        // بررسی عضویت کاربر در ساختمان
        var membership = await _membershipRepository.GetActiveAsync(command.UserId, command.BuildingId);
        if (membership is null)
            return new(false, "شما عضو این ساختمان نیستید و نمی‌توانید آگهی ثبت کنید.");

        // ایجاد آگهی
        var listing = Listing.Create(
            command.BuildingId,
            command.UserId,
            command.Title,
            command.Description,
            command.Type,
            command.Price,
            command.ContactPhone,
            command.ImageUrl);

        await _listingRepository.AddAsync(listing);
        await _listingRepository.SaveChangesAsync();

        return new(true, "آگهی با موفقیت ثبت شد.", listing.Id);
    }
}