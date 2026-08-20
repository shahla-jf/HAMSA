using HAMSA.Domain.Interfaces.Repositories;

namespace HAMSA.Application.Features.Listings.Commands.DeleteListing;

public record DeleteListingCommand(Guid ListingId, Guid UserId);

public record DeleteListingResult(bool Success, string Message);

public class DeleteListingHandler
{
    private readonly IListingRepository _listingRepository;

    public DeleteListingHandler(IListingRepository listingRepository)
    {
        _listingRepository = listingRepository;
    }

    public async Task<DeleteListingResult> HandleAsync(DeleteListingCommand command)
    {
        var listing = await _listingRepository.GetByIdAsync(command.ListingId);
        
        if (listing is null)
            return new(false, "آگهی مورد نظر یافت نشد.");

        if (listing.CreatedByUserId != command.UserId)
            return new(false, "شما فقط مجاز به حذف آگهی‌های خودتان هستید.");

        _listingRepository.Delete(listing);
        await _listingRepository.SaveChangesAsync();

        return new(true, "آگهی با موفقیت حذف شد.");
    }
}