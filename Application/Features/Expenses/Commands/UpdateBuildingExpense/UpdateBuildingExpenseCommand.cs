using HAMSA.Domain.Enums;
using HAMSA.Domain.Interfaces.Repositories;

namespace HAMSA.Application.Features.Expenses.Commands.UpdateBuildingExpense;

public record UpdateBuildingExpenseCommand(
    Guid BuildingId,
    Guid ManagerUserId,
    Guid ExpenseId,
    ExpenseCategory Category,
    string Title,
    decimal Amount);

public record UpdateBuildingExpenseResult(bool Success, string Message);

public class UpdateBuildingExpenseHandler
{
    private readonly IBuildingMembershipRepository _membershipRepository;
    private readonly IBuildingExpenseRepository _expenseRepository;

    public UpdateBuildingExpenseHandler(
        IBuildingMembershipRepository membershipRepository,
        IBuildingExpenseRepository expenseRepository)
    {
        _membershipRepository = membershipRepository;
        _expenseRepository = expenseRepository;
    }

    public async Task<UpdateBuildingExpenseResult> HandleAsync(UpdateBuildingExpenseCommand command)
    {
        var managerId = await _membershipRepository.GetCurrentManagerIdAsync(command.BuildingId);
        if (managerId != command.ManagerUserId)
            return new(false, "فقط مدیر ساختمان اجازه ویرایش هزینه‌ها را دارد.");

        var expense = await _expenseRepository.GetByIdAsync(command.ExpenseId);
        if (expense is null || expense.BuildingId != command.BuildingId)
            return new(false, "هزینه مورد نظر یافت نشد.");

        expense.Update(command.Category, command.Title, command.Amount);
        _expenseRepository.Update(expense);
        await _expenseRepository.SaveChangesAsync();

        return new(true, "هزینه با موفقیت ویرایش شد.");
    }
}