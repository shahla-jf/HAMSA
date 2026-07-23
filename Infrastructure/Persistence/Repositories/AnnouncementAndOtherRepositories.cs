using HAMSA.Domain.Entities;
using HAMSA.Domain.Enums;
using HAMSA.Domain.Interfaces.Repositories;
using HAMSA.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HAMSA.Infrastructure.Persistence.Repositories;

public class AnnouncementRepository : Repository<Announcement>, IAnnouncementRepository
{
    public AnnouncementRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<Announcement>> GetByBuildingIdAsync(Guid buildingId)
        => await _dbSet
            .Include(a => a.ReadRecords)
            .Where(a => a.BuildingId == buildingId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();

    public async Task<bool> IsReadByUserAsync(Guid announcementId, Guid userId)
        => await _context.AnnouncementReads
            .AnyAsync(r => r.AnnouncementId == announcementId && r.UserId == userId);
    
    public async Task<Announcement?> GetWithReadsAsync(Guid announcementId)
        => await _dbSet
            .Include(a => a.ReadRecords)
            .FirstOrDefaultAsync(a => a.Id == announcementId);
    
    public void AddReadRecord(AnnouncementRead readRecord)
        => _context.AnnouncementReads.Add(readRecord);
}

public class NotificationRepository : Repository<Notification>, INotificationRepository
{
    public NotificationRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<Notification>> GetByUserIdAsync(Guid userId)
        => await _dbSet
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();

    public async Task<int> GetUnreadCountAsync(Guid userId)
        => await _dbSet.CountAsync(n => n.UserId == userId && !n.IsRead);
}

public class RepairReportRepository : Repository<RepairReport>, IRepairReportRepository
{
    public RepairReportRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<RepairReport>> GetByBuildingIdAsync(Guid buildingId)
        => await _dbSet
            .Include(r => r.ReportedByUser)
            .Where(r => r.BuildingId == buildingId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

    public async Task<IEnumerable<RepairReport>> GetByStatusAsync(Guid buildingId, RepairStatus status)
        => await _dbSet
            .Include(r => r.ReportedByUser)
            .Where(r => r.BuildingId == buildingId && r.Status == status)
            .ToListAsync();

    public async Task<RepairReport?> GetWithMediaAsync(Guid reportId)
        => await _dbSet
            .Include(r => r.MediaFiles)
            .FirstOrDefaultAsync(r => r.Id == reportId);
}

public class PollRepository : Repository<Poll>, IPollRepository
{
    public PollRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<Poll>> GetActiveByBuildingIdAsync(Guid buildingId)
        => await _dbSet
            .Include(p => p.Options)
            .ThenInclude(o => o.Votes)
            .Where(p => p.BuildingId == buildingId && p.Deadline >= DateTime.UtcNow)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    
    public async Task<IEnumerable<Poll>> GetInactiveByBuildingIdAsync(Guid buildingId)
        => await _dbSet
            .Include(p => p.Options)
            .ThenInclude(o => o.Votes)
            .Where(p => p.BuildingId == buildingId && p.Deadline < DateTime.UtcNow)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

    public async Task<Poll?> GetWithOptionsAndVotesAsync(Guid pollId)
        => await _dbSet
            .Include(p => p.Options).ThenInclude(o => o.Votes)
            .FirstOrDefaultAsync(p => p.Id == pollId);

    public async Task<PollVote?> GetUserVoteAsync(Guid pollId, Guid userId)
        => await _context.PollVotes
            .Include(v => v.PollOption)
            .FirstOrDefaultAsync(v => v.PollOption.PollId == pollId && v.UserId == userId);
    
    public async Task AddVoteAsync(PollVote vote)
    {
        await _context.PollVotes.AddAsync(vote);
    }

    public void RemoveVote(PollVote vote)
    {
        _context.PollVotes.Remove(vote);
    }
}

public class ReservationRepository : Repository<Reservation>, IReservationRepository
{
    public ReservationRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<Reservation>> GetByBuildingAndFacilityAsync(
        Guid buildingId, FacilityType facilityType)
        => await _dbSet
            .Where(r => r.BuildingId == buildingId && r.FacilityType == facilityType)
            .OrderBy(r => r.Date)
            .ToListAsync();

    public async Task<bool> IsReservedAsync(Guid buildingId, FacilityType facilityType, DateTime date)
        => await _dbSet.AnyAsync(r =>
            r.BuildingId == buildingId &&
            r.FacilityType == facilityType &&
            r.Date == date.Date);

    public async Task<IEnumerable<Reservation>> GetByUserIdAsync(Guid userId)
        => await _dbSet
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.Date)
            .ToListAsync();
}
