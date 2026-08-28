using HAMSA.Domain.Entities;
using HAMSA.Domain.Enums;
using HAMSA.Domain.Interfaces.Repositories;
using HAMSA.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HAMSA.Infrastructure.Persistence.Repositories;

public class GroupChallengeRepository : Repository<GroupChallenge>, IGroupChallengeRepository
{
    public GroupChallengeRepository(AppDbContext context) : base(context) { }

    public async Task<GroupChallenge?> GetCurrentAsync(Guid buildingId)
        => await _dbSet
            .Where(c => c.BuildingId == buildingId &&
                        c.Status != GroupChallengeStatus.Completed)
            .OrderByDescending(c => c.CreatedAt)
            .FirstOrDefaultAsync();

    public async Task<GroupChallenge?> GetWithParticipantsAsync(Guid challengeId)
        => await _dbSet
            .Include(c => c.Participants).ThenInclude(p => p.User)
            .FirstOrDefaultAsync(c => c.Id == challengeId);

    public async Task<bool> IsUserRegisteredAsync(Guid challengeId, Guid userId)
        => await _context.GroupChallengeParticipants
            .AnyAsync(p => p.GroupChallengeId == challengeId && p.UserId == userId);
}

public class ChatGroupRepository : Repository<ChatGroup>, IChatGroupRepository
{
    public ChatGroupRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<ChatGroup>> GetByUserIdAsync(Guid userId, Guid buildingId)
        => await _dbSet
            .Where(g => g.BuildingId == buildingId &&
                        g.Members.Any(m => m.UserId == userId))
            .ToListAsync();

    public async Task<ChatGroup?> GetWithMembersAsync(Guid groupId)
        => await _dbSet
            .Include(g => g.Members).ThenInclude(m => m.User)
            .FirstOrDefaultAsync(g => g.Id == groupId);

    public async Task<bool> IsMemberAsync(Guid groupId, Guid userId)
        => await _context.ChatGroupMembers
            .AnyAsync(m => m.ChatGroupId == groupId && m.UserId == userId);
}

public class ChatMessageRepository : Repository<ChatMessage>, IChatMessageRepository
{
    public ChatMessageRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<ChatMessage>> GetByGroupIdAsync(Guid groupId, int page, int pageSize)
        => await _dbSet
            .Include(m => m.Sender)
            .Where(m => m.ChatGroupId == groupId)
            .OrderByDescending(m => m.SentAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
}

public class ListingRepository : Repository<Listing>, IListingRepository
{
    public ListingRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<Listing>> GetByBuildingIdAsync(Guid buildingId)
        => await _dbSet
            .Where(l => l.BuildingId == buildingId)
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync();

    public async Task<IEnumerable<Listing>> GetByUserIdAsync(Guid userId)
        => await _dbSet
            .Where(l => l.CreatedByUserId == userId)
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync();
    
    public async Task<IEnumerable<Listing>> GetByBuildingIdAndUserIdAsync(Guid buildingId, Guid userId)
        => await _dbSet
            .Where(l => l.BuildingId == buildingId && l.CreatedByUserId == userId)
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync();
}

public class ResidentEventRepository : Repository<ResidentEvent>, IResidentEventRepository
{
    public ResidentEventRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<ResidentEvent>> GetByBuildingIdAsync(Guid buildingId)
        => await _dbSet
            .Include(e => e.Sessions)
            .Include(e => e.CreatedByUser)
            .Where(e => e.BuildingId == buildingId)
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync();

    public async Task<IEnumerable<ResidentEvent>> GetByOrganizerAsync(Guid userId)
        => await _dbSet
            .Include(e => e.Registrations)
            .ThenInclude(r => r.User)
            .Include(e => e.Sessions)
            .Where(e => e.CreatedByUserId == userId)
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync();

    public async Task<IEnumerable<ResidentEvent>> GetRegisteredByUserAsync(Guid userId)
        => await _dbSet
            .Include(e => e.Registrations)
            .Include(e => e.Sessions)
            .Include(e => e.CreatedByUser)
            .Where(e => e.Registrations.Any(r => r.UserId == userId))
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync();

    public async Task<ResidentEvent?> GetWithSessionsAsync(Guid eventId)
        => await _dbSet
            .Include(e => e.Sessions)
            .Include(e => e.Registrations)
            .Include(e => e.CreatedByUser)
            .FirstOrDefaultAsync(e => e.Id == eventId);

    public async Task<bool> IsRegisteredAsync(Guid eventId, Guid userId)
        => await _context.EventRegistrations
            .AnyAsync(r => r.ResidentEventId == eventId && r.UserId == userId);
}

public class LocalServiceRepository : Repository<LocalService>, ILocalServiceRepository
{
    public LocalServiceRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<LocalService>> GetByBuildingIdAsync(Guid buildingId)
        => await _dbSet
            .Where(s => s.BuildingId == buildingId)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();

    public async Task<IEnumerable<LocalService>> GetByCategoryAsync(
        Guid buildingId, LocalServiceCategory category)
        => await _dbSet
            .Where(s => s.BuildingId == buildingId && s.Category == category)
            .ToListAsync();

    public async Task<IEnumerable<LocalService>> SearchAsync(Guid buildingId, string keyword)
        => await _dbSet
            .Where(s => s.BuildingId == buildingId &&
                        (s.Title.Contains(keyword) || s.ProviderName.Contains(keyword)))
            .ToListAsync();

    public async Task<LocalService?> GetByIdWithRatingsAsync(Guid id)
        => await _dbSet
            .Include(s => s.Ratings)
            .Include(s => s.Building)
            .Include(s => s.CreatedByUser)
            .FirstOrDefaultAsync(s => s.Id == id);

    public async Task<IEnumerable<LocalService>> GetByBuildingIdsAsync(IEnumerable<Guid> buildingIds)
        => await _dbSet
            .Where(s => buildingIds.Contains(s.BuildingId))
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();
}

public class LocalServiceRatingRepository : Repository<LocalServiceRating>, ILocalServiceRatingRepository
{
    public LocalServiceRatingRepository(AppDbContext context) : base(context) { }

    public async Task<LocalServiceRating?> GetUserRatingAsync(Guid localServiceId, Guid userId)
        => await _dbSet
            .FirstOrDefaultAsync(r => r.LocalServiceId == localServiceId && r.UserId == userId);

    public async Task<double> GetAverageRatingAsync(Guid localServiceId)
    {
        var ratings = await _dbSet
            .Where(r => r.LocalServiceId == localServiceId)
            .Select(r => r.Score)
            .ToListAsync();

        return ratings.Count == 0 ? 0 : ratings.Average();
    }

    public async Task<int> GetRatingCountAsync(Guid localServiceId)
        => await _dbSet
            .CountAsync(r => r.LocalServiceId == localServiceId);
}

public class GroupBuyingRepository : Repository<GroupBuying>, IGroupBuyingRepository
{
    public GroupBuyingRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<GroupBuying>> GetActiveByBuildingIdAsync(Guid buildingId)
        => await _dbSet
            .Include(g => g.Participants)
            .Include(g => g.CreatedByUser)
            .Where(g => g.BuildingId == buildingId && g.Deadline >= DateTime.UtcNow)
            .OrderBy(g => g.Deadline)
            .ToListAsync();

    public async Task<IEnumerable<GroupBuying>> GetByCreatorInBuildingAsync(Guid userId, Guid buildingId)
        => await _dbSet
            .Include(g => g.Participants)
            .Where(g => g.CreatedByUserId == userId && g.BuildingId == buildingId)
            .OrderByDescending(g => g.CreatedAt)
            .ToListAsync();

    public async Task<IEnumerable<GroupBuying>> GetJoinedByUserInBuildingAsync(Guid userId, Guid buildingId)
        => await _dbSet
            .Where(g => g.Participants.Any(p => p.UserId == userId && g.BuildingId == buildingId))
            .OrderByDescending(g => g.CreatedAt)
            .ToListAsync();

    public async Task<bool> IsParticipantAsync(Guid groupBuyingId, Guid userId)
        => await _context.GroupBuyingParticipants
            .AnyAsync(p => p.GroupBuyingId == groupBuyingId && p.UserId == userId);
    
    public async Task<GroupBuying?> GetByIdWithParticipantsAsync(Guid id)
        => await _dbSet
            .Include(g => g.Participants)
            .ThenInclude(p => p.User)
            .FirstOrDefaultAsync(g => g.Id == id);

    public async Task AddParticipantAsync(GroupBuyingParticipant participant)
    {
        await _context.GroupBuyingParticipants.AddAsync(participant);
    }

    public async Task RemoveParticipantAsync(Guid groupBuyingId, Guid userId)
    {
        var participant = await _context.GroupBuyingParticipants
            .FirstOrDefaultAsync(p => p.GroupBuyingId == groupBuyingId && p.UserId == userId);

        if (participant is not null)
            _context.GroupBuyingParticipants.Remove(participant);
    }
}
