using HAMSA.Domain.Interfaces.Repositories;
using HAMSA.Infrastructure.Persistence.Repositories;

namespace HAMSA.Infrastructure.DependencyInjection;

public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IBuildingRepository, BuildingRepository>();
        services.AddScoped<IUnitRepository, UnitRepository>();
        services.AddScoped<IBuildingMembershipRepository, BuildingMembershipRepository>();
        services.AddScoped<IChargeRepository, ChargeRepository>();
        services.AddScoped<ITransactionRepository, TransactionRepository>();
        services.AddScoped<IBuildingExpenseRepository, BuildingExpenseRepository>();
        services.AddScoped<IAnnouncementRepository, AnnouncementRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<IRepairReportRepository, RepairReportRepository>();
        services.AddScoped<IPollRepository, PollRepository>();
        services.AddScoped<IReservationRepository, ReservationRepository>();
        services.AddScoped<IGroupChallengeRepository, GroupChallengeRepository>();
        services.AddScoped<IChatGroupRepository, ChatGroupRepository>();
        services.AddScoped<IChatMessageRepository, ChatMessageRepository>();
        services.AddScoped<IListingRepository, ListingRepository>();
        services.AddScoped<IResidentEventRepository, ResidentEventRepository>();
        services.AddScoped<ILocalServiceRepository, LocalServiceRepository>();
        services.AddScoped<IGroupBuyingRepository, GroupBuyingRepository>();
        services.AddScoped<IBuildingManagerHistoryRepository, BuildingManagerHistoryRepository>();

        return services;
    }
}
