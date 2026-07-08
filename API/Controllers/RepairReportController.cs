using HAMSA.Application.Features.Reports.Commands.CreateRepairReport;
using HAMSA.Application.Features.Reports.Commands.UpdateRepairStatus;
using HAMSA.Application.Features.Reports.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using HAMSA.Domain.Enums;

namespace HAMSA.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RepairReportController : ControllerBase
{
    private readonly CreateRepairReportHandler _createHandler;
    private readonly UpdateRepairStatusHandler _updateHandler;
    private readonly GetRepairReportsHandler _listHandler;

    public RepairReportController(
        CreateRepairReportHandler createHandler,
        UpdateRepairStatusHandler updateHandler,
        GetRepairReportsHandler listHandler)
    {
        _createHandler = createHandler;
        _updateHandler = updateHandler;
        _listHandler = listHandler;
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateRepairReportRequest request)
    {
        var userId = GetUserId();

        var result = await _createHandler.HandleAsync(
            new CreateRepairReportCommand(
                request.BuildingId,
                userId,
                request.Title,
                request.Description,
                request.Priority));

        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("{buildingId}")]
    public async Task<IActionResult> Get(Guid buildingId)
    {
        var userId = GetUserId();

        var result = await _listHandler.HandleAsync(
            new GetRepairReportsQuery(buildingId,userId));

        return Ok(result);
    }

    [HttpPut("{reportId}/status")]
    public async Task<IActionResult> UpdateStatus(
        Guid reportId,
        UpdateRepairStatusRequest request)
    {
        var userId = GetUserId();

        var result = await _updateHandler.HandleAsync(
            new UpdateRepairStatusCommand(
                reportId,
                userId,
                request.Status));

        return result.Success ? Ok(result) : BadRequest(result);
    }

    private Guid GetUserId()
        => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}

public record CreateRepairReportRequest(
    Guid BuildingId,
    string Title,
    string Description,
    RepairPriority Priority);

public record UpdateRepairStatusRequest(
    RepairStatus Status);