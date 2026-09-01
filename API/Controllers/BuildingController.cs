using HAMSA.Application.Features.Buildings.Commands.CreateBuilding;
using HAMSA.Application.Features.Buildings.Commands.TransferManager;
using HAMSA.Application.Features.Buildings.Commands.UpdateBuilding;
using HAMSA.Application.Features.Buildings.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using HAMSA.Application.Features.Buildings.Commands.SelectCurrentBuilding;
using HAMSA.Infrastructure.ExternalServices;
using HAMSA.Domain.Interfaces.Services;

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
    private readonly IFileStorageService _fileStorageService;
    private readonly GetMyRoleInBuildingHandler _getMyRoleHandler;
    private readonly SelectCurrentBuildingHandler _selectCurrentBuildingHandler;
    private readonly GetLastSelectedBuildingHandler _getLastSelectedBuildingHandler;
    private readonly GetBuildingImageHandler _getBuildingImageHandler;

    public BuildingController(
        CreateBuildingHandler createHandler,
        UpdateBuildingHandler updateHandler,
        TransferManagerHandler transferHandler,
        GetBuildingHandler getBuildingHandler,
        GetUserBuildingsHandler getUserBuildingsHandler,
        IFileStorageService fileStorageService,
        GetMyRoleInBuildingHandler getMyRoleHandler,
        SelectCurrentBuildingHandler selectCurrentBuildingHandler,
        GetLastSelectedBuildingHandler getLastSelectedBuildingHandler,
        GetBuildingImageHandler getBuildingImageHandler)
    {
        _createHandler = createHandler;
        _updateHandler = updateHandler;
        _transferHandler = transferHandler;
        _getBuildingHandler = getBuildingHandler;
        _getUserBuildingsHandler = getUserBuildingsHandler;
        _fileStorageService = fileStorageService;
        _getMyRoleHandler = getMyRoleHandler;
        _selectCurrentBuildingHandler = selectCurrentBuildingHandler;
        _getLastSelectedBuildingHandler = getLastSelectedBuildingHandler;
        _getBuildingImageHandler = getBuildingImageHandler;
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

    ///
    /// عکس یک ساختمان
    ///
    [HttpGet("{buildingId}/building-image")]
    public async Task<IActionResult> GetBuildingImage(Guid buildingId)
    {
        var userId = GetUserId();
        var result = await _getBuildingImageHandler.HandleAsync(new GetBuildingImageQuery(userId, buildingId));
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// ایجاد ساختمان جدید
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateBuilding([FromForm] CreateBuildingRequest request)
    {
        var userId = GetUserId();
        string? imageUrl = "https://ham3a.s3.ir-thr-at1.arvanstorage.ir/%D8%B9%DA%A9%D8%B3-%D8%A2%D9%BE%D8%A7%D8%B1%D8%AA%D9%85%D8%A7%D9%86-886x1200.webp?versionId=";
        if(request.Image is not null)
        {
            imageUrl = await _fileStorageService.UploadImageAsync(
                request.Image.OpenReadStream(),
                request.Image.FileName,
                request.Image.ContentType);
        }
        var result = await _createHandler.HandleAsync(new CreateBuildingCommand(
            userId, request.Name, request.BlockCount, request.FloorCount, request.UnitCount,
            request.PostalCode, request.Address, request.Latitude, request.Longitude,
            request.HasGym, request.HasPool, request.HasMeetingHall, request.HasRoofGarden, imageUrl
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
        string? imageUrl = null;
        if(request.Image is not null)
        {
            imageUrl = await _fileStorageService.UploadImageAsync(
                request.Image.OpenReadStream(),
                request.Image.FileName,
                request.Image.ContentType);
        }
        
        var result = await _updateHandler.HandleAsync(new UpdateBuildingCommand(
            buildingId, userId, request.Name, request.BlockCount, request.FloorCount,
            request.UnitCount, request.PostalCode, request.Address, request.Latitude,
            request.Longitude, request.HasGym, request.HasPool, request.HasMeetingHall,
            request.HasRoofGarden, imageUrl
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
    
    [HttpGet("{buildingId}/my-role")]
    public async Task<IActionResult> GetMyRole(Guid buildingId)
    {
        var result = await _getMyRoleHandler.HandleAsync(
            new GetMyRoleInBuildingQuery(
                GetUserId(),
                buildingId));

        return result.Success
            ? Ok(result)
            : BadRequest(result);
    }
    
    [HttpPost("current-building")]
    public async Task<IActionResult> SelectCurrentBuilding(
        [FromBody] SelectCurrentBuildingRequest request)
    {
        var result = await _selectCurrentBuildingHandler.HandleAsync(
            new SelectCurrentBuildingCommand(
                GetUserId(),
                request.BuildingId));

        return result.Success ? Ok(result) : BadRequest(result);
    }
    
    [HttpGet("current-building")]
    public async Task<IActionResult> GetCurrentBuilding()
    {
        var result = await _getLastSelectedBuildingHandler.HandleAsync(
            new GetLastSelectedBuildingQuery(GetUserId()));

        if (result is null)
            return NotFound();

        return Ok(result);
    }

    private Guid GetUserId()
        => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}

// --- Request Models ---
public record CreateBuildingRequest(
    string Name, int BlockCount, int FloorCount, int UnitCount,
    string PostalCode, string Address, double Latitude, double Longitude,
    bool HasGym, bool HasPool, bool HasMeetingHall, bool HasRoofGarden,
    //string FacilitiesPhone, string ManagementPhone, string LobbyPhone,
    IFormFile? Image = null);

public record UpdateBuildingRequest(
    string Name, int BlockCount, int FloorCount, int UnitCount,
    string PostalCode, string Address, double Latitude, double Longitude,
    bool HasGym, bool HasPool, bool HasMeetingHall, bool HasRoofGarden,
    //string FacilitiesPhone, string ManagementPhone, string LobbyPhone,
    IFormFile? Image = null);

public record TransferManagerRequest(Guid NewManagerUserId);

public record SelectCurrentBuildingRequest(Guid BuildingId);