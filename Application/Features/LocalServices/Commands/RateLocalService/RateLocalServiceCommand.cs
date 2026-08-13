using HAMSA.Domain.Entities;
using HAMSA.Domain.Interfaces.Repositories;

namespace HAMSA.Application.Features.LocalServices.Commands.RateLocalService;

public record RateLocalServiceCommand(
    Guid LocalServiceId,
    Guid UserId,
    int Score);

public record RateLocalServiceResult(bool Success, string Message);

public class RateLocalServiceHandler
{
    private readonly ILocalServiceRepository _serviceRepository;
    private readonly ILocalServiceRatingRepository _ratingRepository;
    private readonly IBuildingMembershipRepository _membershipRepository;

    public RateLocalServiceHandler(
        ILocalServiceRepository serviceRepository,
        ILocalServiceRatingRepository ratingRepository,
        IBuildingMembershipRepository membershipRepository)
    {
        _serviceRepository = serviceRepository;
        _ratingRepository = ratingRepository;
        _membershipRepository = membershipRepository;
    }

    public async Task<RateLocalServiceResult> HandleAsync(RateLocalServiceCommand command)
    {
        if (command.Score < 1 || command.Score > 5)
            return new(false, "امتیاز باید بین 1 تا 5 باشد.");

        var service = await _serviceRepository.GetByIdAsync(command.LocalServiceId);
        if (service is null)
            return new(false, "خدمت مورد نظر یافت نشد.");

        // بررسی عضویت کاربر در ساختمان خدمت
        var membership = await _membershipRepository.GetActiveAsync(command.UserId, service.BuildingId);
        if (membership is null)
            return new(false, "شما عضو این ساختمان نیستید.");

        // بررسی نظر قبلی کاربر
        var existingRating = await _ratingRepository.GetUserRatingAsync(command.LocalServiceId, command.UserId);

        if (existingRating is not null)
        {
            // آپدیت نظر قبلی
            existingRating.UpdateScore(command.Score);
            _ratingRepository.Update(existingRating);
        }
        else
        {
            // ایجاد نظر جدید
            var rating = LocalServiceRating.Create(command.LocalServiceId, command.UserId, command.Score);
            await _ratingRepository.AddAsync(rating);
        }

        await _ratingRepository.SaveChangesAsync();

        // محاسبه و آپدیت میانگین نظرات
        var averageRating = await _ratingRepository.GetAverageRatingAsync(command.LocalServiceId);
        var ratingCount = await _ratingRepository.GetRatingCountAsync(command.LocalServiceId);

        service.UpdateRating(averageRating, ratingCount);
        _serviceRepository.Update(service);
        await _serviceRepository.SaveChangesAsync();

        return new(true, "امتیاز شما با موفقیت ثبت شد.");
    }
}