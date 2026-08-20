using System.Security.Claims;
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
    
    public BuildingServicesController(
        CreateLocalServiceHandler createLocalServiceHandler,
        RateLocalServiceHandler rateLocalServiceHandler,
        GetLocalServicesHandler getLocalServicesHandler,
        GetLocalServiceDetailsHandler getLocalServiceDetailsHandler,
        IFileStorageService fileStorageService,
        CreateListingHandler createListingHandler,
        GetListingsHandler getListingsHandler,
        GetListingDetailsHandler getListingDetailsHandler,
        DeleteListingHandler deleteListingHandler)
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


    [HttpDelete("delete-listing")]
    public async Task<IActionResult> DeleteListing(Guid listingId)
    {
        var userId = GetUserId();
        var result = await _deleteListingHandler.HandleAsync(new DeleteListingCommand(listingId, userId));
        return result.Success ? Ok(result) : BadRequest(result);
    }


    [HttpGet("get-listing-details")]
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