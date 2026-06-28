using HAMSA.Domain.Entities;
using HAMSA.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace HAMSA.Infrastructure.Persistence.Repositories;

public class BuildingManagerHistoryRepository
    : Repository<BuildingManagerHistory>, IBuildingManagerHistoryRepository
{
    public BuildingManagerHistoryRepository(AppDbContext context) : base(context) { }

    public async Task EndCurrentManagerAsync(Guid buildingId)
    {
        var current = await _context.BuildingManagerHistories
            .FirstOrDefaultAsync(h => h.BuildingId == buildingId && h.IsCurrent);

        if (current is not null)
        {
            current.End();
            await _context.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<BuildingManagerHistory>> GetHistoryAsync(Guid buildingId)
        => await _context.BuildingManagerHistories
            .Include(h => h.User)
            .Where(h => h.BuildingId == buildingId)
            .OrderByDescending(h => h.StartDate)
            .ToListAsync();
}
