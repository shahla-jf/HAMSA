using HAMSA.Domain.Interfaces.Repositories;

namespace HAMSA.Application.Features.Expenses.Commands.DeleteBuildingExpense;

public record DeleteBuildingExpenseCommand(
    Guid BuildingId,
    Guid ManagerUserId,
    Guid ExpenseId);

public record DeleteBuildingExpenseResult(bool Success, string Message);

public class DeleteBuildingExpenseHandler
{
    private readonly IBuildingMembershipRepository _membershipRepository;
    private readonly IBuildingExpenseRepository _expenseRepository;

    public DeleteBuildingExpenseHandler(
        IBuildingMembershipRepository membershipRepository,
        IBuildingExpenseRepository expenseRepository)
    {
        _membershipRepository = membershipRepository;
        _expenseRepository = expenseRepository;
    }

    public async Task<DeleteBuildingExpenseResult> HandleAsync(DeleteBuildingExpenseCommand command)
    {
        var managerId = await _membershipRepository.GetCurrentManagerIdAsync(command.BuildingId);
        if (managerId != command.ManagerUserId)
            return new(false, "فقط مدیر ساختمان اجازه حذف هزینه‌ها را دارد.");

        var expense = await _expenseRepository.GetByIdAsync(command.ExpenseId);
        if (expense is null || expense.BuildingId != command.BuildingId)
            return new(false, "هزینه مورد نظر یافت نشد.");

        _expenseRepository.Delete(expense);
        await _expenseRepository.SaveChangesAsync();

        return new(true, "هزینه با موفقیت حذف شد.");
    }
}