using System.Security.Claims;
using HAMSA.Application.Features.LocalServices.Commands.CreateLocalService;
using HAMSA.Application.Features.LocalServices.Commands.RateLocalService;
using HAMSA.Application.Features.LocalServices.Queries;
using HAMSA.Domain.Enums;
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

    public BuildingServicesController(
        CreateLocalServiceHandler createLocalServiceHandler,
        RateLocalServiceHandler rateLocalServiceHandler,
        GetLocalServicesHandler getLocalServicesHandler,
        GetLocalServiceDetailsHandler getLocalServiceDetailsHandler)
    {
        _createLocalServiceHandler = createLocalServiceHandler;
        _rateLocalServiceHandler = rateLocalServiceHandler;
        _getLocalServicesHandler = getLocalServicesHandler;
        _getLocalServiceDetailsHandler = getLocalServiceDetailsHandler;
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

    [HttpGet("get-local-service-list")]
    public async Task<IActionResult> GetLocalServices([FromBody] GetLocalServiceListRequest request)
    {
        var userId = GetUserId();
        var result = await _getLocalServicesHandler.HandleAsync(
            new GetLocalServicesQuery(request.BuildingId, userId, request.Category, request.SearchKeyword));
        return Ok(result);
    }

    [HttpGet("{localServiseId}/get-local-service-details")]
    public async Task<IActionResult> GetLocalServiceDetail(Guid localServiseId)
    {
        var userId = GetUserId();
        var result = await _getLocalServiceDetailsHandler.HandleAsync(new GetLocalServiceDetailsQuery(localServiseId, userId));
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

public record  GetLocalServiceListRequest(
    Guid BuildingId,
    LocalServiceCategory? Category = null,
    string? SearchKeyword = null );