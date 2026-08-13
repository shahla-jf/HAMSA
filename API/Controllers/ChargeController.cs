using HAMSA.Application.Features.Charges.Commands.RequestPayment;
using HAMSA.Application.Features.Charges.Commands.SetMonthlyChargeAmount;
using HAMSA.Application.Features.Charges.Commands.UpdateSharedCosts;
using HAMSA.Application.Features.Charges.Commands.VerifyPayment;
using HAMSA.Application.Features.Charges.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using HAMSA.Application.Features.Expenses.Commands.CreateBuildingExpense;
using HAMSA.Application.Features.Expenses.Commands.DeleteBuildingExpense;
using HAMSA.Application.Features.Expenses.Commands.UpdateBuildingExpense;
using HAMSA.Application.Features.Expenses.Queries;
using HAMSA.Domain.Enums;

namespace HAMSA.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ChargeController : ControllerBase
{
    private readonly SetMonthlyChargeAmountHandler _setRateHandler;
    private readonly RequestPaymentHandler _requestPaymentHandler;
    private readonly VerifyPaymentHandler _verifyPaymentHandler;
    private readonly UpdateSharedCostsHandler _updateSharedCostsHandler;
    private readonly GetChargeRatesHandler _getRatesHandler;
    private readonly GetMyCurrentChargeHandler _getMyCurrentChargeHandler;
    private readonly GetPaidChargesHandler _getPaidHandler;
    private readonly GetSharedCostsHandler _getSharedCostsHandler;
    private readonly CreateBuildingExpenseHandler _createBuildingExpenseHandler;
    private readonly DeleteBuildingExpenseHandler _deleteBuildingExpenseHandler;
    private readonly UpdateBuildingExpenseHandler _updateBuildingExpenseHandler;
    private readonly GetBuildingExpensesHandler _getBuildingExpensesHandler;
    private readonly GetMonthlyExpenseSummaryHandler  _getMonthlyExpenseSummaryHandler;
    private readonly GetDashboardFinancialsHandler _getDashboardFinancialsHandler;

    public ChargeController(
        SetMonthlyChargeAmountHandler setRateHandler,
        RequestPaymentHandler requestPaymentHandler,
        VerifyPaymentHandler verifyPaymentHandler,
        UpdateSharedCostsHandler updateSharedCostsHandler,
        GetChargeRatesHandler getRatesHandler,
        GetMyCurrentChargeHandler getMyCurrentChargeHandler,
        GetPaidChargesHandler getPaidHandler,
        GetSharedCostsHandler getSharedCostsHandler,
        CreateBuildingExpenseHandler createBuildingExpenseHandler,
        DeleteBuildingExpenseHandler deleteBuildingExpenseHandler,
        UpdateBuildingExpenseHandler updateBuildingExpenseHandler,
        GetBuildingExpensesHandler getBuildingExpensesHandler,
        GetMonthlyExpenseSummaryHandler getMonthlyExpenseSummaryHandler,
        GetDashboardFinancialsHandler getDashboardFinancialsHandler)
    {
        _setRateHandler = setRateHandler;
        _requestPaymentHandler = requestPaymentHandler;
        _verifyPaymentHandler = verifyPaymentHandler;
        _updateSharedCostsHandler = updateSharedCostsHandler;
        _getRatesHandler = getRatesHandler;
        _getMyCurrentChargeHandler = getMyCurrentChargeHandler;
        _getPaidHandler = getPaidHandler;
        _getSharedCostsHandler = getSharedCostsHandler;
        _createBuildingExpenseHandler = createBuildingExpenseHandler;
        _deleteBuildingExpenseHandler = deleteBuildingExpenseHandler;
        _updateBuildingExpenseHandler = updateBuildingExpenseHandler;
        _getBuildingExpensesHandler = getBuildingExpensesHandler;
        _getMonthlyExpenseSummaryHandler = getMonthlyExpenseSummaryHandler;
        _getDashboardFinancialsHandler = getDashboardFinancialsHandler;
    }

    /// <summary>
    /// مبلغ شارژ ماه جاری و ماه آینده (صفحه مدیر)
    /// </summary>
    [HttpGet("{buildingId}/rates")]
    public async Task<IActionResult> GetRates(Guid buildingId)
    {
        var userId = GetUserId();
        var result = await _getRatesHandler.HandleAsync(new GetChargeRatesQuery(buildingId, userId));
        return result is null ? Forbid() : Ok(result);
    }

    /// <summary>
    /// تعیین مبلغ شارژ یک ماه خاص (فقط مدیر)
    /// </summary>
    [HttpPost("{buildingId}/rates")]
    public async Task<IActionResult> SetRate(Guid buildingId, [FromBody] SetRateRequest request)
    {
        var userId = GetUserId();
        var result = await _setRateHandler.HandleAsync(new SetMonthlyChargeAmountCommand(
            buildingId, userId, request.Year, request.Month, request.Amount));
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// درخواست پرداخت آنلاین شارژ
    /// </summary>
    [HttpPost("{chargeId}/pay")]
    public async Task<IActionResult> RequestPayment(Guid chargeId)
    {
        var userId = GetUserId();
        var callbackBaseUrl = $"{Request.Scheme}://{Request.Host}";
        var result = await _requestPaymentHandler.HandleAsync(
            new RequestPaymentCommand(chargeId, userId, callbackBaseUrl));
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// تایید پرداخت (callback درگاه)
    /// </summary>
    [HttpGet("verify-payment")]
    [AllowAnonymous]
    public async Task<IActionResult> VerifyPayment([FromQuery] Guid transactionId, [FromQuery] string authority)
    {
        var result = await _verifyPaymentHandler.HandleAsync(new VerifyPaymentCommand(transactionId, authority));
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// ویرایش هزینه‌های مشاعات (برق، آب، نظافت، آسانسور) - فقط مدیر
    /// </summary>
    [HttpPut("{buildingId}/shared-costs")]
    public async Task<IActionResult> UpdateSharedCosts(Guid buildingId, [FromBody] UpdateSharedCostsRequest request)
    {
        var userId = GetUserId();
        var result = await _updateSharedCostsHandler.HandleAsync(new UpdateSharedCostsCommand(
            buildingId, userId,
            request.Electricity, request.IsElectricityPaid,
            request.Water, request.IsWaterPaid,
            request.Cleaning, request.IsCleaningPaid,
            request.Elevator, request.IsElevatorPaid));
        return result.Success ? Ok(result) : BadRequest(result);
    }

    ///
    /// مشاهده هزینه های مشاعات
    /// 
    [HttpGet("{buildingId}/shared-costs")]
    public async Task<IActionResult> GetSharedCosts(Guid buildingId)
    {
        var userId = GetUserId();
        var result = await _getSharedCostsHandler.HandleAsync(new GetBuildingSharedCostsQuery(buildingId, userId));
        return result.Success ? Ok(result) : BadRequest(result);
    }
    
    
    /// <summary>
    /// شارژ ماه جاری یک واحد
    /// </summary>
    [HttpGet("unit/{unitId}/current")]
    public async Task<IActionResult> GetMyCurrentCharge(Guid unitId)
    {
        var result = await _getMyCurrentChargeHandler.HandleAsync(new GetMyCurrentChargeQuery(unitId));
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>
    /// شارژهای پرداخت شده ساختمان (فقط مدیر)
    /// </summary>
    [HttpGet("{buildingId}/paid")]
    public async Task<IActionResult> GetUnpaid(Guid buildingId)
    {
        var userId = GetUserId();
        var result = await _getPaidHandler.HandleAsync(new GetPaidChargesQuery(buildingId, userId));
        return Ok(result);
    }


    /// <summary>
    /// ایحاد هزینه جدید
    /// </summary>
    [HttpPost("create-expense")]
    public async Task<IActionResult> CreateExpense([FromBody] CreateBuildingExpenseRequest request)
    {
        var userId = GetUserId();
        var result = await _createBuildingExpenseHandler.HandleAsync(new CreateBuildingExpenseCommand(
            request.BuildingId, userId, request.Category, request.Title, request.Amount));
        return result.Success ? Ok(result) : BadRequest(result);
    }


    /// <summary>
    /// لیست هزینه ها
    /// </summary>
    [HttpGet("{buildingId}/expense")]
    public async Task<IActionResult> GetExpense(Guid buildingId)
    {
        var userId = GetUserId();
        var result = await _getBuildingExpensesHandler.HandleAsync(
            new GetBuildingExpensesQuery(buildingId, userId));
        return Ok(result);
    }
    

    /// <summary>
    /// ویرایش هزینه
    /// </summary>
    [HttpPut("update-expense")]
    public async Task<IActionResult> UpdateExpense([FromBody] UpdateBuildingExpenseRequest request)
    {
        var userId = GetUserId();
        var result = await _updateBuildingExpenseHandler.HandleAsync(new UpdateBuildingExpenseCommand(
            request.BuildingId, userId, request.ExpenseId, request.Category, request.Title, request.Amount));
        return result.Success ? Ok(result) : BadRequest(result);
    }
    

    /// <summary>
    ///    حذف هزینه
    /// </summary>
    [HttpDelete("{buildingId}/delete-expense/{expenseId}")]
    public async Task<IActionResult> DeleteExpense(Guid buildingId, Guid expenseId)
    {
        var userId = GetUserId();
        var result = await _deleteBuildingExpenseHandler.HandleAsync(
            new DeleteBuildingExpenseCommand(buildingId, userId, expenseId));
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// مجموع هزینه های ماه و پرخرج ترین هزینه
    /// </summary>
    [HttpGet("{buildingId}/expense-summary")]
    public async Task<IActionResult> GetExpenseSummary(Guid buildingId)
    {
        var userId = GetUserId();
        var result = await _getMonthlyExpenseSummaryHandler.HandleAsync(
            new GetMonthlyExpenseSummaryQuery(buildingId, userId));
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// لیست هزینه های ماهانه و سالانه برای نمودار
    /// </summary>
    [HttpGet("dashboard-financials")]
    public async Task<IActionResult> GetDashboardFinancials(Guid buildingId)
    {
        var userId = GetUserId();
        var result = await _getDashboardFinancialsHandler.HandleAsync(new GetDashboardFinancialsQuery(buildingId,  userId));
        return result.Success? Ok(result) : BadRequest(result);
    }
    

    private Guid GetUserId()
        => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}

// --- Request Models ---
public record SetRateRequest(int Year, int Month, decimal Amount);
public record UpdateSharedCostsRequest(
    decimal Electricity, bool IsElectricityPaid,
    decimal Water, bool IsWaterPaid,
    decimal Cleaning, bool IsCleaningPaid,
    decimal Elevator, bool IsElevatorPaid);

public record CreateBuildingExpenseRequest(
    Guid BuildingId,
    ExpenseCategory Category,
    string Title,
    decimal Amount);
    
public record UpdateBuildingExpenseRequest(
    Guid BuildingId,
    Guid ExpenseId,
    ExpenseCategory Category,
    string Title,
    decimal Amount);