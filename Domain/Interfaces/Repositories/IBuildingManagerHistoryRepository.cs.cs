using HAMSA.Domain.Entities;
using HAMSA.Domain.Interfaces.Repositories;
using HAMSA.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HAMSA.Domain.Interfaces.Repositories;

public interface IBuildingManagerHistoryRepository : IRepository<BuildingManagerHistory>
{
    Task EndCurrentManagerAsync(Guid buildingId);
    Task<IEnumerable<BuildingManagerHistory>> GetHistoryAsync(Guid buildingId);
}
