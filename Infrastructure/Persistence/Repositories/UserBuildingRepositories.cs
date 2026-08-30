using HAMSA.Domain.Entities;
using HAMSA.Domain.Enums;
using HAMSA.Domain.Interfaces.Repositories;
using HAMSA.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HAMSA.Infrastructure.Persistence.Repositories;

public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(AppDbContext context) : base(context) { }

    public async Task<User?> GetByPhoneNumberAsync(string phoneNumber)
        => await _dbSet.FirstOrDefaultAsync(u => u.PhoneNumber == phoneNumber);

    public async Task<bool> ExistsAsync(string phoneNumber)
        => await _dbSet.AnyAsync(u => u.PhoneNumber == phoneNumber);
}

public class BuildingRepository : Repository<Building>, IBuildingRepository
{
    public BuildingRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<Building>> GetBuildingsByUserIdAsync(Guid userId)
        => await _context.Buildings
            .Where(b => b.Memberships.Any(m => m.UserId == userId && m.IsActive))
            .ToListAsync();

    public async Task<Building?> GetWithDetailsAsync(Guid buildingId)
        => await _dbSet
            .Include(b => b.Units)
            .Include(b => b.Memberships).ThenInclude(m => m.User)
            .FirstOrDefaultAsync(b => b.Id == buildingId);
}

public class UnitRepository : Repository<Unit>, IUnitRepository
{
    public UnitRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<Unit>> GetByBuildingIdAsync(Guid buildingId)
        => await _dbSet.Where(u => u.BuildingId == buildingId).ToListAsync();

    public async Task<Unit?> GetByBlockFloorUnitAsync(Guid buildingId, int block, int floor, int unitNumber)
        => await _dbSet.FirstOrDefaultAsync(u =>
            u.BuildingId == buildingId &&
            u.Block == block &&
            u.Floor == floor &&
            u.UnitNumber == unitNumber);
}

public class BuildingMembershipRepository : Repository<BuildingMembership>, IBuildingMembershipRepository
{
    public BuildingMembershipRepository(AppDbContext context) : base(context) { }

    public async Task<BuildingMembership?> GetByInviteCodeAsync(string inviteCode)
        => await _dbSet.FirstOrDefaultAsync(m => m.InviteCode == inviteCode);

    public async Task<BuildingMembership?> GetActiveAsync(Guid userId, Guid buildingId)
        => await _dbSet.FirstOrDefaultAsync(m =>
            m.UserId == userId &&
            m.BuildingId == buildingId &&
            m.IsActive);

    public async Task<IEnumerable<BuildingMembership>> GetByUnitIdAsync(Guid unitId)
        => await _dbSet
            .Include(m => m.User)
            .Where(m => m.UnitId == unitId && m.IsActive)
            .ToListAsync();

    public async Task<IEnumerable<BuildingMembership>> GetAllByUnitIdAsync(Guid unitId)
        => await _dbSet
            .Include(m => m.User)
            .Where(m => m.UnitId == unitId)
            .ToListAsync();
    
    
    public async Task<IEnumerable<BuildingMembership>> GetPrimaryOwnersByBuildingAsync(Guid buildingId)
    {
        return await _context.BuildingMemberships
            .Include(m => m.User)
            .Include(m => m.Unit)
            .Where(m => m.BuildingId == buildingId && m.Role == UserRole.Owner && m.IsPrimary && m.IsActive)
            .ToListAsync();
    }
    
    public async Task<IEnumerable<BuildingMembership>> GetTenantsByOwnerAsync(Guid ownerUserId, Guid buildingId)
    {
        // واحدهایی که این کاربر مالکشه
        var ownerUnitIds = await _dbSet
            .Where(m => m.UserId == ownerUserId && m.BuildingId == buildingId &&
                        m.Role == Domain.Enums.UserRole.Owner && m.IsActive)
            .Select(m => m.UnitId)
            .ToListAsync();

        // مستاجرین اون واحدها
        return await _dbSet
            .Include(m => m.User)
            .Include(m => m.Unit)
            .Where(m => ownerUnitIds.Contains(m.UnitId) &&
                        m.Role == Domain.Enums.UserRole.Tenant && m.IsActive)
            .ToListAsync();
    }

    public async Task<Guid?> GetCurrentManagerIdAsync(Guid buildingId)
    {
        var history = await _context.BuildingManagerHistories
            .Where(h => h.BuildingId == buildingId && h.IsCurrent)
            .FirstOrDefaultAsync();
        return history?.UserId;
    }
    
    public async Task<IEnumerable<BuildingMembership>> GetByUserAsync(Guid userId)
    {
        return await _dbSet
            .Where(x => x.UserId == userId && x.IsActive)
            .ToListAsync();
    }
    
    public async Task<BuildingMembership?> GetLastSelectedAsync(Guid userId)
    {
        return await _dbSet
            .Include(x => x.Building)
            .Where(x => x.UserId == userId &&
                        x.IsActive &&
                        x.LastSelectedAt != null)
            .OrderByDescending(x => x.LastSelectedAt)
            .FirstOrDefaultAsync();
    }
    
    public async Task<IEnumerable<BuildingMembership>> GetCoMembersByUserInBuildingAsync(Guid userId, Guid buildingId)
    {
        var query = from userMembership in _dbSet
            join coMember in _dbSet 
                on new { userMembership.UnitId, userMembership.Role } 
                equals new { coMember.UnitId, coMember.Role }
            where userMembership.UserId == userId 
                  && userMembership.BuildingId == buildingId
                  && userMembership.IsActive  // خود کاربر هم باید عضویت فعال داشته باشد
                  && coMember.UserId != userId 
                  && coMember.IsActive
            select coMember;

        return await query
            .Include(m => m.User)
            .Include(m => m.Unit)
            .Distinct()
            .ToListAsync();
    }
    
    
    public async Task<IEnumerable<BuildingMembership>> GetUserUnitsInBuildingAsync(Guid userId, Guid buildingId)
    {
        return await _dbSet
            .Include(m => m.Unit)
            .Where(m => m.UserId == userId && 
                        m.BuildingId == buildingId && 
                        m.IsActive && m.Unit != null)
            .ToListAsync();
    }
    
    public async Task<IEnumerable<BuildingMembership>> GetByUnitIdsAsync(IEnumerable<Guid> unitIds)
    {
        var unitIdList = unitIds.ToList();
    
        return await _dbSet
            .Include(m => m.User)
            .Where(m => m.UnitId.HasValue && unitIdList.Contains(m.UnitId.Value))
            .ToListAsync();
    }
}
