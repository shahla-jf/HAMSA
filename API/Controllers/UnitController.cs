using HAMSA.Application.Features.Units.Commands.AddOwner;
using HAMSA.Application.Features.Units.Commands.AddTenant;
using HAMSA.Application.Features.Units.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using HAMSA.Application.Features.Units.Commands.RemoveOneOwner;
using HAMSA.Application.Features.Units.Commands.RemoveOneTenant;
using HAMSA.Application.Features.Units.Commands.RemoveOwner;
using HAMSA.Application.Features.Units.Commands.RemoveTenant;
using HAMSA.Application.Features.Units.Commands.UpdateOwner;
using HAMSA.Application.Features.Units.Commands.UpdateTenant;

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
    private readonly RemoveOwnerHandler _removeOwnerHandler;
    private readonly RemoveTenantHandler _removeTenantHandler;
    private readonly RemoveOneOwnerHandler _removeOneOwnerHandler;
    private readonly RemoveOneTenantHandler _removeOneTenantHandler;
    private readonly EditOwnerHandler _editOwnerHandler;
    private readonly EditTenantHandler _editTenantHandler;
    private readonly GetPrimaryOwnersHandler _getPrimaryOwnersHandler;
    private readonly GetCoMembersHandler _getCoMembersHandler;

    public UnitController(
        AddOwnerHandler addOwnerHandler,
        AddTenantHandler addTenantHandler,
        GetUnitsByBuildingHandler getUnitsHandler,
        GetMyTenantsHandler getMyTenantsHandler,
        RemoveOwnerHandler removeOwnerHandler,
        RemoveTenantHandler removeTenantHandler,
        RemoveOneOwnerHandler removeOneOwnerHandler,
        RemoveOneTenantHandler removeOneTenantHandler,
        EditOwnerHandler editOwnerHandler,
        EditTenantHandler editTenantHandler,
        GetPrimaryOwnersHandler getPrimaryOwnersHandler,
        GetCoMembersHandler getCoMembersHandler)
    {
        _addOwnerHandler = addOwnerHandler;
        _addTenantHandler = addTenantHandler;
        _getUnitsHandler = getUnitsHandler;
        _getMyTenantsHandler = getMyTenantsHandler;
        _removeOwnerHandler =  removeOwnerHandler;
        _removeTenantHandler = removeTenantHandler;
        _removeOneOwnerHandler = removeOneOwnerHandler;
        _removeOneTenantHandler = removeOneTenantHandler;
        _editOwnerHandler = editOwnerHandler;
        _editTenantHandler = editTenantHandler;
        _getPrimaryOwnersHandler = getPrimaryOwnersHandler;
        _getCoMembersHandler = getCoMembersHandler;
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
            request.Block, request.Floor, request.UnitNumber
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
            request.Block, request.Floor, request.UnitNumber
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

    /// <summary>
    /// حذف مالک های یک واحد از ساختمان (فقط مدیر)
    /// </summary>
    [HttpDelete("remove-owner")]
    public async Task<IActionResult> RemoveOwner([FromBody]  RemoveOwnerRequest request)
    {
        var userId = GetUserId();
        var result = await _removeOwnerHandler.HandleAsync(new RemoveOwnerCommand(
            request.BuildingId, userId, request.Block, request.Floor, request.UnitNumber));
        return result.Success ? Ok(result) : BadRequest(result);
    }


    /// <summary>
    /// حذف مستاجرهای یک واحد از ساختمان (توسط مالک همان واحد)
    /// </summary>
    [HttpDelete("remove-tenant")]
    public async Task<IActionResult> RemoveTenant([FromBody] RemoveTenantRequest request)
    {
        var userId = GetUserId();
        var result = await _removeTenantHandler.HandleAsync(new RemoveTenantCommand(
            request.BuildingId, userId, request.Block, request.Floor, request.UnitNumber));
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// حذف یکی از مالک های یک واحد
    /// </summary>
    [HttpDelete("remove-one-owner")]
    public async Task<IActionResult> RemoveOneOwner([FromBody] RemoveOneOwnerRequest request)
    {
        var userId = GetUserId();
        var result = await _removeOneOwnerHandler.HandleAsync(new RemoveOneOwnerCommand(
            request.BuildingId, userId, request.OwnerPhoneNumber,
            request.Block, request.Floor, request.UnitNumber));
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// حذف یکی از مستاجرهای یک واحد
    /// </summary>
    [HttpDelete("remove-one-tenant")]
    public async Task<IActionResult> RemoveOneTenant([FromBody] RemoveOneTenantRequest request)
    {
        var userId = GetUserId();
        var result = await _removeOneTenantHandler.HandleAsync(new RemoveOneTenantCommand(
            request.BuildingId, userId, request.TenantPhoneNumber,
            request.Block, request.Floor, request.UnitNumber));
        return result.Success ? Ok(result) : BadRequest(result);
    }


    [HttpPut("edit-owner")]
    public async Task<IActionResult> UpdateOwnerDate([FromBody] UpdateOwnerRequest request)
    {
        var userId = GetUserId();
        var result = await _editOwnerHandler.HandleAsync(new EditOwnerCommand(
            request.BuildingId, userId, request.Block, request.Floor, request.UnitNumber,
            request.StartDate,  request.EndDate, request.NewBlock, request.NewFloor,
            request.NewUnitNumber, request.TargetUserId));
        return result.Success? Ok(result) : BadRequest(result);
    }
    
    [HttpPut("edit-tenant")]
    public async Task<IActionResult> UpdateTenantDate([FromBody] UpdateTenantRequest request)
    {
        var userId = GetUserId();
        var result = await _editTenantHandler.HandleAsync(new EditTenantCommand(
            request.BuildingId, userId, request.Block, request.Floor, request.UnitNumber,
            request.StartDate,  request.EndDate, request.NewBlock, request.NewFloor,
            request.NewUnitNumber, request.TargetUserId));
        return result.Success? Ok(result) : BadRequest(result);
    }

    [HttpGet("{buildingId}/active-primary-owners")]
    public async Task<IActionResult> GetActivePrimaryOwners(Guid buildingId)
    {
        var userId = GetUserId();
        var result = await _getPrimaryOwnersHandler.HandleAsync(new GetPrimaryOwnersQuery(buildingId, userId));
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("{buildingId}/comembers")]
    public async Task<IActionResult> GetComMembers(Guid buildingId)
    {
        var userId = GetUserId();
        var result = await _getCoMembersHandler.HandleAsync(new GetCoMembersQuery(buildingId, userId));
        return result.Success ? Ok(result) : BadRequest(result);
    }

    private Guid GetUserId()
        => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}

// --- Request Models ---
public record AddOwnerRequest(
    string OwnerPhoneNumber, int Block, int Floor, int UnitNumber);

public record AddTenantRequest(
    string TenantPhoneNumber, int Block, int Floor, int UnitNumber);

public record RemoveOwnerRequest(
    Guid BuildingId,
    int Block, int Floor, int UnitNumber
    );

public record RemoveTenantRequest(
    Guid BuildingId,
    int Block, int Floor, int UnitNumber);
    
public record RemoveOneOwnerRequest(
    string OwnerPhoneNumber,
    Guid BuildingId,
    int Block, int Floor, int UnitNumber);
    
public record  RemoveOneTenantRequest(
    string TenantPhoneNumber,
    Guid BuildingId,
    int Block, int Floor, int UnitNumber);
    
public record UpdateOwnerRequest(
    Guid BuildingId,
    int Block, int Floor, int UnitNumber,
    DateTime? StartDate,
    DateTime? EndDate,
    int? NewBlock, int? NewFloor, int? NewUnitNumber,
    Guid? TargetUserId);
    
public record UpdateTenantRequest(
    Guid BuildingId,
    int Block, int Floor, int UnitNumber,
    DateTime? StartDate,
    DateTime? EndDate,
    int? NewBlock, int? NewFloor, int? NewUnitNumber,
    Guid? TargetUserId);