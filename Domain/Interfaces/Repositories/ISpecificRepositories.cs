using System.Linq.Expressions;
using HAMSA.Domain.Entities;
using HAMSA.Domain.Enums;
using HAMSA.Domain.Interfaces.Repositories;

namespace HAMSA.Domain.Interfaces.Repositories;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByPhoneNumberAsync(string phoneNumber);
    Task<bool> ExistsAsync(string phoneNumber);
}

public interface IBuildingRepository : IRepository<Building>
{
    Task<IEnumerable<Building>> GetBuildingsByUserIdAsync(Guid userId);
    Task<Building?> GetWithDetailsAsync(Guid buildingId);
}

public interface IUnitRepository : IRepository<Unit>
{
    Task<IEnumerable<Unit>> GetByBuildingIdAsync(Guid buildingId);
    Task<Unit?> GetByBlockFloorUnitAsync(Guid buildingId, int block, int floor, int unitNumber);
}

public interface IBuildingMembershipRepository : IRepository<BuildingMembership>
{
    Task<BuildingMembership?> GetByInviteCodeAsync(string inviteCode);
    Task<BuildingMembership?> GetActiveAsync(Guid userId, Guid buildingId);
    Task<IEnumerable<BuildingMembership>> GetByUnitIdAsync(Guid unitId);
    Task<IEnumerable<BuildingMembership>> GetAllByUnitIdAsync(Guid buildingId);
    Task<IEnumerable<BuildingMembership>> GetTenantsByOwnerAsync(Guid ownerUserId, Guid buildingId);
    Task<Guid?> GetCurrentManagerIdAsync(Guid buildingId);
    Task<IEnumerable<BuildingMembership>> GetByUserAsync(Guid userId);
    Task<BuildingMembership?> GetLastSelectedAsync(Guid userId);
    Task<IEnumerable<BuildingMembership>> GetPrimaryOwnersByBuildingAsync(Guid buildingId);
    Task<IEnumerable<BuildingMembership>> GetCoMembersByUserInBuildingAsync(Guid userId, Guid buildingId);
    Task<IEnumerable<BuildingMembership>> GetUserUnitsInBuildingAsync(Guid userId, Guid buildingId);
    Task<IEnumerable<BuildingMembership>> GetByUnitIdsAsync(IEnumerable<Guid> unitIds);
}

public interface IChargeRepository : IRepository<Charge>
{
    Task<IEnumerable<Charge>> GetByUnitIdAsync(Guid unitId);
    Task<Charge?> GetByUnitAndMonthAsync(Guid unitId, int year, int month);
    Task<IEnumerable<Charge>> GetPaidByBuildingAsync(Guid buildingId);
    Task<decimal> GetTotalIncomeAsync(Guid buildingId, int year, int month);
    Task<decimal> GetMonthlyIncomeAsync(Guid buildingId, int year, int month);
    Task<IEnumerable<Charge>> GetByUnitIdsAndMonthAsync(IEnumerable<Guid> unitIds, int year, int month);
}

public interface ITransactionRepository : IRepository<Transaction>
{
    Task<IEnumerable<Transaction>> GetByUserIdAsync(Guid userId);
    Task<IEnumerable<Transaction>> GetByUserIdAndDateRangeAsync(Guid userId, DateTime from, DateTime to);
    Task<bool> ExistsAsync(Expression<Func<Transaction, bool>> predicate);
    Task<IEnumerable<Transaction>> GetPendingByChargeIdsAsync(IEnumerable<Guid> chargeIds);
}

public interface IBuildingExpenseRepository : IRepository<BuildingExpense>
{
    Task<IEnumerable<BuildingExpense>> GetByBuildingIdAsync(Guid buildingId);
    Task<BuildingExpense?> GetByIdAsync(Guid id);
    Task<IEnumerable<BuildingExpense>> GetByBuildingAndMonthAsync(Guid buildingId, int year, int month);
    Task<decimal> GetTotalExpensesAsync(Guid buildingId, int year, int month);
    Task<(string Title, decimal Amount)> GetHighestExpenseAsync(Guid buildingId, int year, int month);
    Task<Dictionary<(int Year, int Month), decimal>> GetMonthlyExpensesForYearAsync(Guid buildingId, int year);
    Task<Dictionary<int, decimal>> GetYearlyExpensesAsync(Guid buildingId);
}

public interface IAnnouncementRepository : IRepository<Announcement>
{
    Task<IEnumerable<Announcement>> GetByBuildingIdAsync(Guid buildingId);
    Task<bool> IsReadByUserAsync(Guid announcementId, Guid userId);
    Task<Announcement?> GetWithReadsAsync(Guid announcementId);
    void AddReadRecord(AnnouncementRead readRecord);
}

public interface INotificationRepository : IRepository<Notification>
{
    Task<IEnumerable<Notification>> GetByUserIdAsync(Guid userId);
    Task<int> GetUnreadCountAsync(Guid userId);
}

public interface IRepairReportRepository : IRepository<RepairReport>
{
    Task<IEnumerable<RepairReport>> GetByBuildingIdAsync(Guid buildingId);
    Task<IEnumerable<RepairReport>> GetByStatusAsync(Guid buildingId, RepairStatus status);
    Task<RepairReport?> GetWithMediaAsync(Guid reportId);
}

public interface IPollRepository : IRepository<Poll>
{
    Task<IEnumerable<Poll>> GetActiveByBuildingIdAsync(Guid buildingId);
    Task<IEnumerable<Poll>> GetInactiveByBuildingIdAsync(Guid buildingId);
    Task<Poll?> GetWithOptionsAndVotesAsync(Guid pollId);
    Task<PollVote?> GetUserVoteAsync(Guid pollId, Guid userId);
    Task AddVoteAsync(PollVote vote);
    void RemoveVote(PollVote vote);
}

public interface IReservationRepository : IRepository<Reservation>
{
    Task<IEnumerable<Reservation>> GetByBuildingAndFacilityAsync(Guid buildingId, FacilityType facilityType);
    Task<bool> IsReservedAsync(Guid buildingId, FacilityType facilityType, DateTime date);
    Task<IEnumerable<Reservation>> GetByUserIdAsync(Guid userId);
}

public interface IGroupChallengeRepository : IRepository<GroupChallenge>
{
    Task<GroupChallenge?> GetCurrentAsync(Guid buildingId);
    Task<GroupChallenge?> GetWithParticipantsAsync(Guid challengeId);
    Task<bool> IsUserRegisteredAsync(Guid challengeId, Guid userId);
    Task<GroupChallenge?> GetCurrentWithParticipantsAsync(Guid buildingId);
}

public interface IChatGroupRepository : IRepository<ChatGroup>
{
    Task<IEnumerable<ChatGroup>> GetByUserIdAsync(Guid userId, Guid buildingId);
    Task<ChatGroup?> GetWithMembersAsync(Guid groupId);
    Task<bool> IsMemberAsync(Guid groupId, Guid userId);
}

public interface IChatMessageRepository : IRepository<ChatMessage>
{
    Task<IEnumerable<ChatMessage>> GetByGroupIdAsync(Guid groupId, int page, int pageSize);
}

public interface IListingRepository : IRepository<Listing>
{
    Task<IEnumerable<Listing>> GetByBuildingIdAsync(Guid buildingId);
    Task<IEnumerable<Listing>> GetByUserIdAsync(Guid userId);
    Task<IEnumerable<Listing>> GetByBuildingIdAndUserIdAsync(Guid buildingId, Guid userId);
}

public interface IResidentEventRepository : IRepository<ResidentEvent>
{
    Task<IEnumerable<ResidentEvent>> GetByBuildingIdAsync(Guid buildingId);
    Task<IEnumerable<ResidentEvent>> GetByOrganizerAsync(Guid userId);
    Task<IEnumerable<ResidentEvent>> GetRegisteredByUserAsync(Guid userId);
    Task<ResidentEvent?> GetWithSessionsAsync(Guid eventId);
    Task<bool> IsRegisteredAsync(Guid eventId, Guid userId);
}

public interface ILocalServiceRepository : IRepository<LocalService>
{
    Task<IEnumerable<LocalService>> GetByBuildingIdAsync(Guid buildingId);
    Task<IEnumerable<LocalService>> GetByCategoryAsync(Guid buildingId, LocalServiceCategory category);
    Task<IEnumerable<LocalService>> SearchAsync(Guid buildingId, string keyword);
    Task<LocalService?> GetByIdWithRatingsAsync(Guid id);
    Task<IEnumerable<LocalService>> GetByBuildingIdsAsync(IEnumerable<Guid> buildingIds);
}

public interface ILocalServiceRatingRepository : IRepository<LocalServiceRating>
{
    Task<LocalServiceRating?> GetUserRatingAsync(Guid localServiceId, Guid userId);
    Task<double> GetAverageRatingAsync(Guid localServiceId);
    Task<int> GetRatingCountAsync(Guid localServiceId);
}

public interface IGroupBuyingRepository : IRepository<GroupBuying>
{
    Task<IEnumerable<GroupBuying>> GetActiveByBuildingIdAsync(Guid buildingId);
    Task<IEnumerable<GroupBuying>> GetByCreatorInBuildingAsync(Guid userId, Guid  buildingId);
    Task<IEnumerable<GroupBuying>> GetJoinedByUserInBuildingAsync(Guid userId, Guid buildingId);
    Task<bool> IsParticipantAsync(Guid groupBuyingId, Guid userId);
    Task<GroupBuying?> GetByIdWithParticipantsAsync(Guid id);
    Task AddParticipantAsync(GroupBuyingParticipant participant);
    Task RemoveParticipantAsync(Guid groupBuyingId, Guid userId);
}
