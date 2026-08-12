using HAMSA.Domain.Entities;
using HAMSA.Domain.Interfaces.Repositories;
using HAMSA.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HAMSA.Infrastructure.Persistence.Repositories;

public class ChargeRepository : Repository<Charge>, IChargeRepository
{
    public ChargeRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<Charge>> GetByUnitIdAsync(Guid unitId)
        => await _dbSet.Where(c => c.UnitId == unitId)
            .OrderByDescending(c => c.Year).ThenByDescending(c => c.Month)
            .ToListAsync();

    public async Task<Charge?> GetByUnitAndMonthAsync(Guid unitId, int year, int month)
        => await _dbSet.FirstOrDefaultAsync(c =>
            c.UnitId == unitId && c.Year == year && c.Month == month);

    public async Task<IEnumerable<Charge>> GetPaidByBuildingAsync(Guid buildingId)
        => await _dbSet
            .Include(c => c.Unit)
            .Where(c => c.BuildingId == buildingId && c.IsPaid)
            .ToListAsync();

    public async Task<decimal> GetTotalIncomeAsync(Guid buildingId, int year, int month)
        => await _dbSet
            .Where(c => c.BuildingId == buildingId &&
                        c.Year == year && c.Month == month && c.IsPaid)
            .SumAsync(c => c.Amount + c.PenaltyAmount);
}

public class TransactionRepository : Repository<Transaction>, ITransactionRepository
{
    public TransactionRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<Transaction>> GetByUserIdAsync(Guid userId)
        => await _dbSet
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

    public async Task<IEnumerable<Transaction>> GetByUserIdAndDateRangeAsync(
        Guid userId, DateTime from, DateTime to)
        => await _dbSet
            .Where(t => t.UserId == userId &&
                        t.CreatedAt >= from && t.CreatedAt <= to)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
}

public class BuildingExpenseRepository : Repository<BuildingExpense>, IBuildingExpenseRepository
{
    public BuildingExpenseRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<BuildingExpense>> GetByBuildingAndMonthAsync(
        Guid buildingId, int year, int month)
        => await _dbSet
            .Where(e => e.BuildingId == buildingId && e.Year == year && e.Month == month)
            .ToListAsync();

    public async Task<decimal> GetTotalExpensesAsync(Guid buildingId, int year, int month)
        => await _dbSet
            .Where(e => e.BuildingId == buildingId && e.Year == year && e.Month == month)
            .SumAsync(e => e.Amount);

    public async Task<(string Title, decimal Amount)> GetHighestExpenseAsync(
        Guid buildingId, int year, int month)
    {
        var expense = await _dbSet
            .Where(e => e.BuildingId == buildingId && e.Year == year && e.Month == month)
            .OrderByDescending(e => e.Amount)
            .FirstOrDefaultAsync();

        return expense is null ? ("ندارد", 0) : (expense.Title, expense.Amount);
    }
}
