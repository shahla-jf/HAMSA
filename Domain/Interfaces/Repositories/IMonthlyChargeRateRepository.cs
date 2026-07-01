using HAMSA.Domain.Entities;

namespace HAMSA.Domain.Interfaces.Repositories;

public interface IMonthlyChargeRateRepository : IRepository<MonthlyChargeRate>
{
    Task<MonthlyChargeRate?> GetByMonthAsync(Guid buildingId, int year, int month);
}
