using System.Linq.Expressions;
using HAMSA.Domain.Entities;
using HAMSA.Domain.Enums;
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
    {
        return await _dbSet
            .Include(c => c.Unit)
            .Include(c => c.Transactions)
            .Where(c => c.BuildingId == buildingId && c.IsPaid)
            .ToListAsync();
    }

    public async Task<decimal> GetTotalIncomeAsync(Guid buildingId, int year, int month)
        => await _dbSet
            .Where(c => c.BuildingId == buildingId &&
                        c.Year == year && c.Month == month && c.IsPaid)
            .SumAsync(c => c.Amount + c.PenaltyAmount);
    
    public async Task<decimal> GetMonthlyIncomeAsync(Guid buildingId, int year, int month)
    {
        return await _dbSet
            .Where(c => c.BuildingId == buildingId && c.Year == year && c.Month == month && c.IsPaid)
            .SumAsync(c => (decimal?)(c.Amount + c.PenaltyAmount)) ?? 0;
    }
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
    
    public async Task<bool> ExistsAsync(Expression<Func<Transaction, bool>> predicate)
    {
        return await _dbSet.AnyAsync(predicate);
    }
    
    public async Task<Transaction?> GetPendingByChargeIdAsync(Guid chargeId)
    {
        return await _dbSet
            .FirstOrDefaultAsync(t => t.ChargeId == chargeId && t.Status == TransactionStatus.PendingVerification);
    }
}

public class BuildingExpenseRepository : Repository<BuildingExpense>, IBuildingExpenseRepository
{
    public BuildingExpenseRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<BuildingExpense>> GetByBuildingIdAsync(Guid buildingId)
        => await _dbSet
            .Include(e => e.CreatedByUser)
            .Where(e => e.BuildingId == buildingId)
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync();

    public async Task<BuildingExpense?> GetByIdAsync(Guid id)
        => await _dbSet
            .Include(e => e.CreatedByUser)
            .FirstOrDefaultAsync(e => e.Id == id);
    
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
    
    public async Task<Dictionary<(int Year, int Month), decimal>> GetMonthlyExpensesForYearAsync(Guid buildingId, int year)
    {
        var expenses = await _dbSet
            .Where(e => e.BuildingId == buildingId && e.Year == year)
            .GroupBy(e => new { e.Year, e.Month })
            .Select(g => new { g.Key.Year, g.Key.Month, Total = g.Sum(e => e.Amount) })
            .ToListAsync();

        return expenses.ToDictionary(x => (x.Year, x.Month), x => x.Total);
    }

    public async Task<Dictionary<int, decimal>> GetYearlyExpensesAsync(Guid buildingId)
    {
        var expenses = await _dbSet
            .Where(e => e.BuildingId == buildingId)
            .GroupBy(e => e.Year)
            .Select(g => new { Year = g.Key, Total = g.Sum(e => e.Amount) })
            .ToListAsync();

        return expenses.ToDictionary(x => x.Year, x => x.Total);
    }
}
