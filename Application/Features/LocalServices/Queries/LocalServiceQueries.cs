using HAMSA.Domain.Entities;
using HAMSA.Domain.Enums;
using HAMSA.Domain.Interfaces.Repositories;

namespace HAMSA.Application.Features.LocalServices.Queries;

/// <summary>
/// لیست خدمات محلی نزدیک
/// </summary>
//-------------Query--------------
public record GetLocalServicesQuery(
    Guid BuildingId,
    Guid UserId);

public record LocalServiceListItem(
    Guid Id,
    string Title,
    LocalServiceCategory Category,
    string CategoryName,
    double AverageRating,
    int RatingCount);

public class GetLocalServicesHandler
{
    private readonly ILocalServiceRepository _serviceRepository;
    private readonly IBuildingRepository _buildingRepository;
    private readonly IBuildingMembershipRepository _membershipRepository;

    private const double NearbyRadiusKm = 2.0; // شعاع 2 کیلومتر

    public GetLocalServicesHandler(
        ILocalServiceRepository serviceRepository,
        IBuildingRepository buildingRepository,
        IBuildingMembershipRepository membershipRepository)
    {
        _serviceRepository = serviceRepository;
        _buildingRepository = buildingRepository;
        _membershipRepository = membershipRepository;
    }

    public async Task<IEnumerable<LocalServiceListItem>> HandleAsync(GetLocalServicesQuery query)
    {
        var membership = await _membershipRepository.GetActiveAsync(query.UserId, query.BuildingId);
        if (membership is null)
            return Enumerable.Empty<LocalServiceListItem>();

        // دریافت ساختمان کاربر
        var userBuilding = await _buildingRepository.GetByIdAsync(query.BuildingId);
        if (userBuilding is null)
            return Enumerable.Empty<LocalServiceListItem>();

        // دریافت همه ساختمان‌ها
        var allBuildings = await _buildingRepository.GetAllAsync();

        // پیدا کردن ساختمان‌های نزدیک (شامل ساختمان خود کاربر)
        var nearbyBuildingIds = allBuildings
            .Where(b => CalculateDistance(
                userBuilding.Latitude, userBuilding.Longitude,
                b.Latitude, b.Longitude) <= NearbyRadiusKm)
            .Select(b => b.Id)
            .ToList();

        // دریافت خدمات ساختمان‌های نزدیک
        var services = await _serviceRepository.GetByBuildingIdsAsync(nearbyBuildingIds);

        return services.Select(s => new LocalServiceListItem(
            s.Id,
            s.Title,
            s.Category,
            GetCategoryDisplayName(s.Category),
            s.AverageRating,
            s.RatingCount));
    }

    private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 6371; // شعاع زمین به کیلومتر
        var dLat = ToRadians(lat2 - lat1);
        var dLon = ToRadians(lon2 - lon1);
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return R * c;
    }

    private double ToRadians(double degrees) => degrees * Math.PI / 180;

    private string GetCategoryDisplayName(LocalServiceCategory category) => category switch
    {
        LocalServiceCategory.Cleaning => "نظافت و خدمات منزل",
        LocalServiceCategory.Education => "آموزش و کلاس‌ها",
        LocalServiceCategory.Transportation => "حمل و نقل و رفاهی",
        LocalServiceCategory.Health => "بهداشت و سلامت",
        LocalServiceCategory.Beauty => "زیبایی و آرایشی",
        LocalServiceCategory.Legal => "حقوقی و اداری",
        LocalServiceCategory.Construction => "ساختمانی و بازسازی",
        _ => "نامشخص"
    };
}





/// <summary>
/// جزئیات خدمات محلی
/// </summary>
///

// -----------Query-----------
public record GetLocalServiceDetailsQuery(
    Guid LocalServiceId,
    Guid UserId);

public record LocalServiceDetails(
    bool Success,
    string Message,
    Guid Id,
    LocalServiceCategory Category,
    string CategoryName,
    string Title,
    string Description,
    string ProviderName,
    string ContactPhone,
    string WorkingHours,
    double AverageRating,
    int RatingCount);

public class GetLocalServiceDetailsHandler
{
    private readonly ILocalServiceRepository _serviceRepository;
    private readonly IBuildingMembershipRepository _membershipRepository;

    public GetLocalServiceDetailsHandler(
        ILocalServiceRepository serviceRepository,
        IBuildingMembershipRepository membershipRepository)
    {
        _serviceRepository = serviceRepository;
        _membershipRepository = membershipRepository;
    }

    public async Task<LocalServiceDetails> HandleAsync(GetLocalServiceDetailsQuery query)
    {
        var service = await _serviceRepository.GetByIdWithRatingsAsync(query.LocalServiceId);
        
        if (service is null)
            return new(false, "خدمت مورد نظر یافت نشد.", Guid.Empty, default, "", "", "", "", "", "", 0, 0);

        // بررسی دسترسی: کاربر باید عضو ساختمان خدمت یا ساختمان‌های نزدیک باشد
        var membership = await _membershipRepository.GetActiveAsync(query.UserId, service.BuildingId);
        
        // اگر عضو ساختمان خود خدمت نیست، چک می‌کنیم آیا عضو ساختمان‌های نزدیک است
        // برای سادگی، فعلاً فقط اعضای ساختمان خود خدمت دسترسی دارند
        // می‌توانید منطق ساختمان‌های نزدیک را اینجا هم پیاده‌سازی کنید
        
        if (membership is null)
            return new(false, "شما به این خدمت دسترسی ندارید.", Guid.Empty, default, "", "", "", "", "", "", 0, 0);

        return new(
            true,
            "جزئیات خدمت با موفقیت دریافت شد.",
            service.Id,
            service.Category,
            GetCategoryDisplayName(service.Category),
            service.Title,
            service.Description,
            service.ProviderName,
            service.ContactPhone,
            service.WorkingHours,
            service.AverageRating,
            service.RatingCount);
    }

    private string GetCategoryDisplayName(LocalServiceCategory category) => category switch
    {
        LocalServiceCategory.Cleaning => "نظافت و خدمات منزل",
        LocalServiceCategory.Education => "آموزش و کلاس‌ها",
        LocalServiceCategory.Transportation => "حمل و نقل و رفاهی",
        LocalServiceCategory.Health => "بهداشت و سلامت",
        LocalServiceCategory.Beauty => "زیبایی و آرایشی",
        LocalServiceCategory.Legal => "حقوقی و اداری",
        LocalServiceCategory.Construction => "ساختمانی و بازسازی",
        _ => "نامشخص"
    };
}