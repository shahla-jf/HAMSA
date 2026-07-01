using HAMSA.Domain.Entities;
using HAMSA.Domain.Interfaces.Repositories;
using HAMSA.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HAMSA.Infrastructure.Persistence.Repositories;

public class MonthlyChargeRateRepository : Repository<MonthlyChargeRate>, IMonthlyChargeRateRepository
{
    public MonthlyChargeRateRepository(AppDbContext context) : base(context) { }

    public async Task<MonthlyChargeRate?> GetByMonthAsync(Guid buildingId, int year, int month)
        => await _dbSet.FirstOrDefaultAsync(r =>
            r.BuildingId == buildingId && r.Year == year && r.Month == month);
}
