using HAMSA.Application.Features.Charges.Commands.RequestPayment;
using HAMSA.Application.Features.Charges.Commands.SetMonthlyChargeAmount;
using HAMSA.Application.Features.Charges.Commands.UpdateSharedCosts;
using HAMSA.Application.Features.Charges.Commands.VerifyPayment;
using HAMSA.Application.Features.Charges.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

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

    public ChargeController(
        SetMonthlyChargeAmountHandler setRateHandler,
        RequestPaymentHandler requestPaymentHandler,
        VerifyPaymentHandler verifyPaymentHandler,
        UpdateSharedCostsHandler updateSharedCostsHandler,
        GetChargeRatesHandler getRatesHandler,
        GetMyCurrentChargeHandler getMyCurrentChargeHandler,
        GetPaidChargesHandler getPaidHandler,
        GetSharedCostsHandler getSharedCostsHandler)
    {
        _setRateHandler = setRateHandler;
        _requestPaymentHandler = requestPaymentHandler;
        _verifyPaymentHandler = verifyPaymentHandler;
        _updateSharedCostsHandler = updateSharedCostsHandler;
        _getRatesHandler = getRatesHandler;
        _getMyCurrentChargeHandler = getMyCurrentChargeHandler;
        _getPaidHandler = getPaidHandler;
        _getSharedCostsHandler = getSharedCostsHandler;
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
