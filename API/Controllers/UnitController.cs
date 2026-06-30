using HAMSA.Application.Features.Units.Commands.AddOwner;
using HAMSA.Application.Features.Units.Commands.AddTenant;
using HAMSA.Application.Features.Units.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HAMSA.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UnitController : ControllerBase
{
    private readonly AddOwnerHandler _addOwnerHandler;
    private readonly AddTenantHandler _addTenantHandler;
    private readonly GetUnitsByBuildingHandler _getUnitsHandler;
    private readonly GetMyTenantsHandler _getMyTenantsHandler;

    public UnitController(
        AddOwnerHandler addOwnerHandler,
        AddTenantHandler addTenantHandler,
        GetUnitsByBuildingHandler getUnitsHandler,
        GetMyTenantsHandler getMyTenantsHandler)
    {
        _addOwnerHandler = addOwnerHandler;
        _addTenantHandler = addTenantHandler;
        _getUnitsHandler = getUnitsHandler;
        _getMyTenantsHandler = getMyTenantsHandler;
    }

    /// <summary>
    /// افزودن مالک جدید به یک واحد (فقط مدیر)
    /// اگر واحد وجود نداشته باشد ساخته می‌شود، در غیر این صورت مالکیت منتقل می‌شود
    /// </summary>
    [HttpPost("{buildingId}/owners")]
    public async Task<IActionResult> AddOwner(Guid buildingId, [FromBody] AddOwnerRequest request)
    {
        var userId = GetUserId();
        var result = await _addOwnerHandler.HandleAsync(new AddOwnerCommand(
            buildingId, userId, request.OwnerPhoneNumber,
            request.Block, request.Floor, request.UnitNumber, request.IsResident
        ));
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// افزودن مستاجر جدید (فقط توسط مالک واحد)
    /// </summary>
    [HttpPost("{buildingId}/tenants")]
    public async Task<IActionResult> AddTenant(Guid buildingId, [FromBody] AddTenantRequest request)
    {
        var userId = GetUserId();
        var result = await _addTenantHandler.HandleAsync(new AddTenantCommand(
            buildingId, userId, request.TenantPhoneNumber,
            request.Block, request.Floor, request.UnitNumber, request.StartDate
        ));
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// لیست همه واحدهای ساختمان به همراه مالک/مستاجر (فقط مدیر)
    /// </summary>
    [HttpGet("{buildingId}")]
    public async Task<IActionResult> GetUnits(Guid buildingId)
    {
        var userId = GetUserId();
        var result = await _getUnitsHandler.HandleAsync(new GetUnitsByBuildingQuery(buildingId, userId));
        return Ok(result);
    }

    /// <summary>
    /// لیست مستاجرین یک مالک در این ساختمان
    /// </summary>
    [HttpGet("{buildingId}/my-tenants")]
    public async Task<IActionResult> GetMyTenants(Guid buildingId)
    {
        var userId = GetUserId();
        var result = await _getMyTenantsHandler.HandleAsync(new GetMyTenantsQuery(buildingId, userId));
        return Ok(result);
    }

    private Guid GetUserId()
        => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}

// --- Request Models ---
public record AddOwnerRequest(
    string OwnerPhoneNumber, int Block, int Floor, int UnitNumber, bool IsResident);

public record AddTenantRequest(
    string TenantPhoneNumber, int Block, int Floor, int UnitNumber, DateTime StartDate);
