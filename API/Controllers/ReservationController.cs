using HAMSA.Application.Features.Reservations.Commands.CreateReservation;
using HAMSA.Application.Features.Reservations.Queries;
using HAMSA.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HAMSA.API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class ReservationController : ControllerBase
{
    private readonly CreateReservationHandler _createHandler;
    private readonly GetReservedDatesHandler _getReservedDatesHandler;
    private readonly GetMyReservationsHandler _myHandler;

    public ReservationController(
        CreateReservationHandler createHandler,
        GetReservedDatesHandler getReservedDatesHandler,
        GetMyReservationsHandler myHandler)
    {
        _createHandler = createHandler;
        _getReservedDatesHandler = getReservedDatesHandler;
        _myHandler = myHandler;
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateReservationRequest request)
    {
        var result = await _createHandler.HandleAsync(
            new CreateReservationCommand(
                request.BuildingId,
                GetUserId(),
                request.FacilityType,
                request.Date));

        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// روزهای رزرو شده یک امکان
    /// </summary>
    [HttpGet("reserved-dates")]
    public async Task<IActionResult> GetReservedDates(
        [FromQuery] Guid buildingId,
        [FromQuery] FacilityType facilityType)
    {
        var result = await _getReservedDatesHandler.HandleAsync(
            new GetReservedDatesQuery(buildingId, facilityType));

        return Ok(result);
    }

    [HttpGet("my")]
    public async Task<IActionResult> My()
    {
        var result = await _myHandler.HandleAsync(
            new GetMyReservationsQuery(GetUserId()));

        return Ok(result);
    }

    private Guid GetUserId()
        => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}

public record CreateReservationRequest(
    Guid BuildingId,
    FacilityType FacilityType,
    DateTime Date);