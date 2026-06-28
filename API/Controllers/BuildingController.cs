using HAMSA.Application.Features.Buildings.Commands.CreateBuilding;
using HAMSA.Application.Features.Buildings.Commands.TransferManager;
using HAMSA.Application.Features.Buildings.Commands.UpdateBuilding;
using HAMSA.Application.Features.Buildings.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HAMSA.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BuildingController : ControllerBase
{
    private readonly CreateBuildingHandler _createHandler;
    private readonly UpdateBuildingHandler _updateHandler;
    private readonly TransferManagerHandler _transferHandler;
    private readonly GetBuildingHandler _getBuildingHandler;
    private readonly GetUserBuildingsHandler _getUserBuildingsHandler;

    public BuildingController(
        CreateBuildingHandler createHandler,
        UpdateBuildingHandler updateHandler,
        TransferManagerHandler transferHandler,
        GetBuildingHandler getBuildingHandler,
        GetUserBuildingsHandler getUserBuildingsHandler)
    {
        _createHandler = createHandler;
        _updateHandler = updateHandler;
        _transferHandler = transferHandler;
        _getBuildingHandler = getBuildingHandler;
        _getUserBuildingsHandler = getUserBuildingsHandler;
    }

    /// <summary>
    /// ساختمان‌هایی که کاربر عضوشونه
    /// </summary>
    [HttpGet("my-buildings")]
    public async Task<IActionResult> GetMyBuildings()
    {
        var userId = GetUserId();
        var result = await _getUserBuildingsHandler.HandleAsync(new GetUserBuildingsQuery(userId));
        return Ok(result);
    }

    /// <summary>
    /// اطلاعات یک ساختمان
    /// </summary>
    [HttpGet("{buildingId}")]
    public async Task<IActionResult> GetBuilding(Guid buildingId)
    {
        var userId = GetUserId();
        var result = await _getBuildingHandler.HandleAsync(new GetBuildingQuery(buildingId, userId));
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>
    /// ایجاد ساختمان جدید
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateBuilding([FromBody] CreateBuildingRequest request)
    {
        var userId = GetUserId();
        var result = await _createHandler.HandleAsync(new CreateBuildingCommand(
            userId, request.Name, request.BlockCount, request.FloorCount, request.UnitCount,
            request.PostalCode, request.Address, request.Latitude, request.Longitude,
            request.HasGym, request.HasPool, request.HasMeetingHall, request.HasRoofGarden,
            request.FacilitiesPhone, request.ManagementPhone, request.LobbyPhone, request.ImageUrl
        ));
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// ویرایش اطلاعات ساختمان (فقط مدیر)
    /// </summary>
    [HttpPut("{buildingId}")]
    public async Task<IActionResult> UpdateBuilding(Guid buildingId, [FromBody] UpdateBuildingRequest request)
    {
        var userId = GetUserId();
        var result = await _updateHandler.HandleAsync(new UpdateBuildingCommand(
            buildingId, userId, request.Name, request.BlockCount, request.FloorCount,
            request.UnitCount, request.PostalCode, request.Address, request.Latitude,
            request.Longitude, request.HasGym, request.HasPool, request.HasMeetingHall,
            request.HasRoofGarden, request.FacilitiesPhone, request.ManagementPhone,
            request.LobbyPhone, request.ImageUrl
        ));
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// انتقال مدیریت ساختمان (فقط مدیر فعلی)
    /// </summary>
    [HttpPost("{buildingId}/transfer-manager")]
    public async Task<IActionResult> TransferManager(Guid buildingId, [FromBody] TransferManagerRequest request)
    {
        var userId = GetUserId();
        var result = await _transferHandler.HandleAsync(
            new TransferManagerCommand(buildingId, userId, request.NewManagerUserId));
        return result.Success ? Ok(result) : BadRequest(result);
    }

    private Guid GetUserId()
        => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}

// --- Request Models ---
public record CreateBuildingRequest(
    string Name, int BlockCount, int FloorCount, int UnitCount,
    string PostalCode, string Address, double Latitude, double Longitude,
    bool HasGym, bool HasPool, bool HasMeetingHall, bool HasRoofGarden,
    string FacilitiesPhone, string ManagementPhone, string LobbyPhone,
    string? ImageUrl = null);

public record UpdateBuildingRequest(
    string Name, int BlockCount, int FloorCount, int UnitCount,
    string PostalCode, string Address, double Latitude, double Longitude,
    bool HasGym, bool HasPool, bool HasMeetingHall, bool HasRoofGarden,
    string FacilitiesPhone, string ManagementPhone, string LobbyPhone,
    string? ImageUrl = null);

public record TransferManagerRequest(Guid NewManagerUserId);
