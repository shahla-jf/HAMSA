using HAMSA.Domain.Entities;
using HAMSA.Domain.Enums;
using HAMSA.Domain.Interfaces.Repositories;

namespace HAMSA.Application.Features.Buildings.Commands.TransferManager;

// ------- Command -------
public record TransferManagerCommand(
    Guid BuildingId,
    Guid CurrentManagerId,
    Guid NewManagerUserId
);

// ------- Result -------
public record TransferManagerResult(bool Success, string Message);

// ------- Handler -------
public class TransferManagerHandler
{
    private readonly IBuildingRepository _buildingRepository;
    private readonly IBuildingMembershipRepository _membershipRepository;
    private readonly IUserRepository _userRepository;
    private readonly IBuildingManagerHistoryRepository _managerHistoryRepository;

    public TransferManagerHandler(
        IBuildingRepository buildingRepository,
        IBuildingMembershipRepository membershipRepository,
        IUserRepository userRepository,
        IBuildingManagerHistoryRepository managerHistoryRepository)
    {
        _buildingRepository = buildingRepository;
        _membershipRepository = membershipRepository;
        _userRepository = userRepository;
        _managerHistoryRepository = managerHistoryRepository;
    }

    public async Task<TransferManagerResult> HandleAsync(TransferManagerCommand command)
    {
        // بررسی مدیر فعلی
        var currentManagerId = await _membershipRepository.GetCurrentManagerIdAsync(command.BuildingId);
        if (currentManagerId != command.CurrentManagerId)
            return new TransferManagerResult(false, "شما مدیر این ساختمان نیستید");

        // بررسی وجود مدیر جدید
        var newManager = await _userRepository.GetByIdAsync(command.NewManagerUserId);
        if (newManager is null)
            return new TransferManagerResult(false, "کاربر مورد نظر یافت نشد");

        // بررسی اینکه مدیر جدید عضو ساختمان باشه
        var newManagerMembership = await _membershipRepository.GetActiveAsync(
            command.NewManagerUserId, command.BuildingId);
        if (newManagerMembership is null)
            return new TransferManagerResult(false, "کاربر مورد نظر عضو این ساختمان نیست");

        // پایان دوره مدیر فعلی
        await _managerHistoryRepository.EndCurrentManagerAsync(command.BuildingId);

        // شروع دوره مدیر جدید
        var newHistory = BuildingManagerHistory.Create(command.BuildingId, command.NewManagerUserId);
        await _managerHistoryRepository.AddAsync(newHistory);
        await _managerHistoryRepository.SaveChangesAsync();

        return new TransferManagerResult(true,
            $"مدیریت ساختمان با موفقیت به {newManager.FirstName} {newManager.LastName} منتقل شد");
    }
}
