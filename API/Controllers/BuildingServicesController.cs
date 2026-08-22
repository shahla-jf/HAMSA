using System.Security.Claims;
using HAMSA.Application.Features.GroupBuying.Commands.CreateGroupBuying;
using HAMSA.Application.Features.GroupBuying.Commands.DeleteGroupBuying;
using HAMSA.Application.Features.GroupBuying.Queries;
using HAMSA.Application.Features.GroupBuyings.Commands.JoinGroupBuying;
using HAMSA.Application.Features.GroupBuyings.Commands.LeaveGroupBuying;
using HAMSA.Application.Features.Listings.Commands.CreateListing;
using HAMSA.Application.Features.Listings.Commands.DeleteListing;
using HAMSA.Application.Features.Listings.Queries;
using HAMSA.Application.Features.LocalServices.Commands.CreateLocalService;
using HAMSA.Application.Features.LocalServices.Commands.RateLocalService;
using HAMSA.Application.Features.LocalServices.Queries;
using HAMSA.Domain.Enums;
using HAMSA.Domain.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HAMSA.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BuildingServicesController: ControllerBase
{
    private readonly CreateLocalServiceHandler _createLocalServiceHandler;
    private readonly RateLocalServiceHandler _rateLocalServiceHandler;
    private readonly GetLocalServicesHandler  _getLocalServicesHandler;
    private readonly GetLocalServiceDetailsHandler _getLocalServiceDetailsHandler;
    private readonly IFileStorageService  _fileStorageService;
    private readonly CreateListingHandler _createListingHandler;
    private readonly GetListingsHandler _getListingsHandler;
    private readonly GetListingDetailsHandler _getListingDetailsHandler;
    private  readonly DeleteListingHandler _deleteListingHandler;
    private readonly GetMyListingsHandler _getMyListingsHandler;
    private readonly CreateGroupBuyingHandler _createGroupBuyingHandler;
    private readonly DeleteGroupBuyingHandler _deleteGroupBuyingHandler;
    private readonly JoinGroupBuyingHandler _joinGroupBuyingHandler;
    private readonly LeaveGroupBuyingHandler _leaveGroupBuyingHandler;
    private readonly GetMyGroupBuyingsHandler _getMyGroupBuyingsHandler;
    private readonly GetBuildingGroupBuyingsHandler _getBuildingGroupBuyingsHandler;
    private readonly GetJoinedGroupBuyingsHandler _getJoinedGroupBuyingsHandler;
    
    public BuildingServicesController(
        CreateLocalServiceHandler createLocalServiceHandler,
        RateLocalServiceHandler rateLocalServiceHandler,
        GetLocalServicesHandler getLocalServicesHandler,
        GetLocalServiceDetailsHandler getLocalServiceDetailsHandler,
        IFileStorageService fileStorageService,
        CreateListingHandler createListingHandler,
        GetListingsHandler getListingsHandler,
        GetListingDetailsHandler getListingDetailsHandler,
        DeleteListingHandler deleteListingHandler,
        GetMyListingsHandler getMyListingsHandler,
        CreateGroupBuyingHandler createGroupBuyingHandler,
        DeleteGroupBuyingHandler deleteGroupBuyingHandler,
        JoinGroupBuyingHandler joinGroupBuyingHandler,
        LeaveGroupBuyingHandler leaveGroupBuyingHandler,
        GetMyGroupBuyingsHandler getMyGroupBuyingsHandler,
        GetBuildingGroupBuyingsHandler getBuildingGroupBuyingsHandler,
        GetJoinedGroupBuyingsHandler getJoinedGroupBuyingsHandler)
    {
        _createLocalServiceHandler = createLocalServiceHandler;
        _rateLocalServiceHandler = rateLocalServiceHandler;
        _getLocalServicesHandler = getLocalServicesHandler;
        _getLocalServiceDetailsHandler = getLocalServiceDetailsHandler;
        _fileStorageService = fileStorageService;
        _createListingHandler = createListingHandler;
        _getListingsHandler = getListingsHandler;
        _getListingDetailsHandler = getListingDetailsHandler;
        _deleteListingHandler = deleteListingHandler;
        _getMyListingsHandler = getMyListingsHandler;
        _createGroupBuyingHandler = createGroupBuyingHandler;
        _deleteGroupBuyingHandler = deleteGroupBuyingHandler;
        _joinGroupBuyingHandler = joinGroupBuyingHandler;
        _leaveGroupBuyingHandler = leaveGroupBuyingHandler;
        _getMyGroupBuyingsHandler = getMyGroupBuyingsHandler;
        _getBuildingGroupBuyingsHandler = getBuildingGroupBuyingsHandler;
        _getJoinedGroupBuyingsHandler = getJoinedGroupBuyingsHandler;

    }

    [HttpPost("create-local-service")]
    public async Task<IActionResult> CreateLocalService([FromBody] CreateLocalServiceRequest request)
    {
        var userId = GetUserId();
        var result = await _createLocalServiceHandler.HandleAsync(new CreateLocalServiceCommand(request.BuildingId, userId,
            request.Category, request.Title, request.Description, request.ProviderName, request.ContactPhone, request.WorkingHours));
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("rate-local-service")]
    public async Task<IActionResult> RateLocalService([FromBody] RateLocalServiceRequest request)
    {
        var userId = GetUserId();
        var result = await _rateLocalServiceHandler.HandleAsync(new RateLocalServiceCommand(request.LocalServiceId, userId, request.Score));
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("{buildingId}/get-local-service-list")]
    public async Task<IActionResult> GetLocalServices(Guid buildingId)
    {
        var userId = GetUserId();
        var result = await _getLocalServicesHandler.HandleAsync(
            new GetLocalServicesQuery(buildingId, userId));
        return Ok(result);
    }

    [HttpGet("{localServiseId}/get-local-service-details")]
    public async Task<IActionResult> GetLocalServiceDetail(Guid localServiseId)
    {
        var userId = GetUserId();
        var result = await _getLocalServiceDetailsHandler.HandleAsync(new GetLocalServiceDetailsQuery(localServiseId, userId));
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("create-listing")]
    public async Task<IActionResult> CreateListing([FromForm] CreateListingRequest request)
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
        
        var result = await _createListingHandler.HandleAsync(new CreateListingCommand(
            request.BuildingId, userId, request.Title, request.Description,
            request.Type, request.Price, request.ContactPhone, imageUrl));
        
        return result.Success ? Ok(result) : BadRequest(result);
    }


    [HttpDelete("{listingId}/delete-listing")]
    public async Task<IActionResult> DeleteListing(Guid listingId)
    {
        var userId = GetUserId();
        var result = await _deleteListingHandler.HandleAsync(new DeleteListingCommand(listingId, userId));
        return result.Success ? Ok(result) : BadRequest(result);
    }


    [HttpGet("{listingId}/get-listing-details")]
    public async Task<IActionResult> GetListingDetails(Guid listingId)
    {
        var userId = GetUserId();
        var result = await _getListingDetailsHandler.HandleAsync(new GetListingDetailsQuery(listingId, userId));
        return Ok(result);
    }

    [HttpGet("{buildingId}/get-listing-list")]
    public async Task<IActionResult> GetListingList(Guid buildingId)
    {
        var userId = GetUserId();
        var result = await _getListingsHandler.HandleAsync(new GetListingsQuery(buildingId, userId));
        return Ok(result);
    }

    [HttpGet("{buildingId}/get-my-listing")]
    public async Task<IActionResult> GetMyListing(Guid buildingId)
    {
        var userId = GetUserId();
        var result = await _getMyListingsHandler.HandleAsync(new GetMyListingsQuery(buildingId, userId));
        return Ok(result);
    }
    
    
    
    [HttpPost("create-group-buying")]
    public async Task<IActionResult> CreateGroupBuying([FromBody] CreateGroupBuyingRequest request)
    {
        var userId = GetUserId();
        var result = await _createGroupBuyingHandler.HandleAsync(new CreateGroupBuyingCommand(
            request.BuildingId, userId, request.Title,
            request.MinimumQuantity, request.Price, request.Deadline, request.ContactPhone));
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("delete-group-buying/{groupBuyingId}")]
    public async Task<IActionResult> DeleteGroupBuying(Guid groupBuyingId)
    {
        var userId = GetUserId();
        var result = await _deleteGroupBuyingHandler.HandleAsync(new DeleteGroupBuyingCommand(groupBuyingId, userId));
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("join-group-buying/{groupBuyingId}")]
    public async Task<IActionResult> JoinGroupBuying(Guid groupBuyingId)
    {
        var userId = GetUserId();
        var result = await _joinGroupBuyingHandler.HandleAsync(new JoinGroupBuyingCommand(groupBuyingId, userId));
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("leave-group-buying/{groupBuyingId}")]
    public async Task<IActionResult> LeaveGroupBuying(Guid groupBuyingId)
    {
        var userId = GetUserId();
        var result = await _leaveGroupBuyingHandler.HandleAsync(new LeaveGroupBuyingCommand(groupBuyingId, userId));
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("{buildingId}/get-my-group-buyings")]
    public async Task<IActionResult> GetMyGroupBuyings(Guid buildingId)
    {
        var userId = GetUserId();
        var result = await _getMyGroupBuyingsHandler.HandleAsync(new GetMyGroupBuyingsQuery(userId, buildingId));
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("{buildingId}/get-building-group-buyings")]
    public async Task<IActionResult> GetBuildingGroupBuyings(Guid buildingId)
    {
        var userId = GetUserId();
        var result = await _getBuildingGroupBuyingsHandler.HandleAsync(new GetBuildingGroupBuyingsQuery(buildingId, userId));
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("{buildingId}/get-joined-group-buyings")]
    public async Task<IActionResult> GetJoinedGroupBuyings(Guid buildingId)
    {
        var userId = GetUserId();
        var result = await _getJoinedGroupBuyingsHandler.HandleAsync(new GetJoinedGroupBuyingsQuery(userId, buildingId));
        return result.Success ? Ok(result) : BadRequest(result);
    }
    
    
    private Guid GetUserId()
        => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}

// --- Request Models ---
public record CreateLocalServiceRequest(
    Guid BuildingId,
    LocalServiceCategory Category,
    string Title,
    string Description,
    string ProviderName,
    string ContactPhone,
    string WorkingHours);
    
public record RateLocalServiceRequest(Guid LocalServiceId, int Score);
    
public record CreateListingRequest(
    Guid BuildingId,
    string Title,
    string Description,
    ListingType Type,
    decimal? Price,
    string ContactPhone,
    IFormFile? Image = null);
    
public record CreateGroupBuyingRequest(
    Guid BuildingId,
    string Title,
    int MinimumQuantity,
    decimal Price,
    DateTime Deadline,
    string ContactPhone);