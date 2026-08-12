using HAMSA.Domain.Enums;
using HAMSA.Domain.Entities;
using HAMSA.Domain.Interfaces.Repositories;

namespace HAMSA.Application.Features.Expenses.Commands.CreateBuildingExpense;

public record CreateBuildingExpenseCommand(
    Guid BuildingId,
    Guid ManagerUserId,
    ExpenseCategory Category,
    string Title,
    decimal Amount);

public record CreateBuildingExpenseResult(bool Success, string Message, Guid? ExpenseId = null);

public class CreateBuildingExpenseHandler
{
    private readonly IBuildingMembershipRepository _membershipRepository;
    private readonly IBuildingExpenseRepository _expenseRepository;

    public CreateBuildingExpenseHandler(
        IBuildingMembershipRepository membershipRepository,
        IBuildingExpenseRepository expenseRepository)
    {
        _membershipRepository = membershipRepository;
        _expenseRepository = expenseRepository;
    }

    public async Task<CreateBuildingExpenseResult> HandleAsync(CreateBuildingExpenseCommand command)
    {
        var managerId = await _membershipRepository.GetCurrentManagerIdAsync(command.BuildingId);
        if (managerId != command.ManagerUserId)
            return new(false, "فقط مدیر ساختمان می‌تواند هزینه جدید ثبت کند.");

        var expense = BuildingExpense.Create(
            command.BuildingId,
            command.ManagerUserId,
            command.Category,
            command.Title,
            command.Amount);

        await _expenseRepository.AddAsync(expense);
        await _expenseRepository.SaveChangesAsync();

        return new(true, "هزینه با موفقیت ثبت شد.", expense.Id);
    }
}