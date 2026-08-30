using System.Security.Claims;
using HAMSA.Application.Features.GroupChallenge.Commands.MarkChallengeCompleted;
using HAMSA.Application.Features.GroupChallenge.Commands.RegisterForChallenge;
using HAMSA.Application.Features.GroupChallenge.Queries;
using HAMSA.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HAMSA.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]

public class ChallengeController : ControllerBase
{
    private readonly RegisterForChallengeHandler _registerForChallengeHandler;
    private readonly MarkChallengeCompletedHandler _markChallengeCompletedHandler;
    private readonly GetChallengeStatusHandler _getChallengeStatusHandler;
    private readonly GetChallengeDetailsHandler _getChallengeDetailsHandler;

    public ChallengeController(
        RegisterForChallengeHandler registerForChallengeHandler,
        MarkChallengeCompletedHandler markChallengeCompletedHandler,
        GetChallengeStatusHandler getChallengeStatusHandler,
        GetChallengeDetailsHandler getChallengeDetailsHandler)
    {
        _registerForChallengeHandler = registerForChallengeHandler;
        _markChallengeCompletedHandler = markChallengeCompletedHandler;
        _getChallengeStatusHandler = getChallengeStatusHandler;
        _getChallengeDetailsHandler = getChallengeDetailsHandler;
    }
    
    [HttpGet("{buildingId}/get-challenge-status")]
    public async Task<IActionResult> GetChallengeStatus(Guid buildingId)
    {
        var userId = GetUserId();
        var result = await _getChallengeStatusHandler.HandleAsync(
            new GetChallengeStatusQuery(buildingId, userId));
        return Ok(result);
    }

    [HttpPost("register-for-challenge")]
    public async Task<IActionResult> RegisterForChallenge([FromBody] RegisterForChallengeRequest request)
    {
        var userId = GetUserId();
        var result = await _registerForChallengeHandler.HandleAsync(new RegisterForChallengeCommand(
            request.BuildingId, userId, request.FullName,
            request.Age, request.Gender, request.SportsBackground));
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("{buildingId}/get-challenge-details")]
    public async Task<IActionResult> GetChallengeDetails(Guid buildingId)
    {
        var userId = GetUserId();
        var result = await _getChallengeDetailsHandler.HandleAsync(
            new GetChallengeDetailsQuery(buildingId, userId));
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("complete-challenge")]
    public async Task<IActionResult> CompleteChallenge(Guid buildingId)
    {
        var userId = GetUserId();
        var result = await _markChallengeCompletedHandler.HandleAsync(
            new MarkChallengeCompletedCommand(buildingId, userId));
        return result.Success ? Ok(result) : BadRequest(result);
    }
    
    
    private Guid GetUserId()
        => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}

// --- Request Models ---
public record RegisterForChallengeRequest(
    Guid BuildingId,
    string? FullName,
    int Age,
    Gender Gender,
    SportsBackground SportsBackground);