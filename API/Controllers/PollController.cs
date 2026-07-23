using System.Security.Claims;
using HAMSA.Application.Features.Polls.Commands.VotePoll;
using HAMSA.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HAMSA.API.Controllers;


[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PollController : ControllerBase
{
    private readonly CreatePollHandler _createPollHandler;
    private readonly GetActivePollsHandler _getActivePollsHandler;
    private readonly GetInActivePollsHandler _getInActivePollsHandler;
    private readonly VotePollHandler _votePollHandler;
    
    public PollController(
        CreatePollHandler createPollHandler,
        GetActivePollsHandler getActivePollsHandler,
        GetInActivePollsHandler getInActivePollsHandler,
        VotePollHandler votePollHandler)
    {
        _createPollHandler = createPollHandler;
        _getActivePollsHandler = getActivePollsHandler;
        _getInActivePollsHandler =  getInActivePollsHandler;
        _votePollHandler = votePollHandler;
    }

    [HttpPost]
    public async Task<IActionResult> CreatePoll([FromBody] CreatePollRequest createPollRequest)
    {
        var userId = GetUserId();
        var result = await _createPollHandler.HandleAsync(new CreatePollCommand(
            createPollRequest.BuildingId, userId, createPollRequest.Title,
            createPollRequest.Description, createPollRequest.Audience,
            createPollRequest.Deadline, createPollRequest.Options));
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("{buildingId}/ActivePolls")]
    public async Task<IActionResult> GetActivePolls(Guid buildingId)
    {
        var userId = GetUserId();
        var result = await _getActivePollsHandler.HandleAsync(buildingId, userId);
        return Ok(result);
    }
    
    [HttpGet("{buildingId}/InActivePolls")]
    public async Task<IActionResult> GetInActivePolls(Guid buildingId)
    {
        var userId = GetUserId();
        var result = await _getInActivePollsHandler.HandleAsync(buildingId, userId);
        return Ok(result);
    }

    [HttpPost("Vote")]
    public async Task<IActionResult> VotePoll([FromBody] VotePollRequest votePollRequest)
    {
        var userId = GetUserId();
        var result = await _votePollHandler.HandleAsync(new VotePollCommand(
            votePollRequest.PollId,
            votePollRequest.OptionId,
            userId));
        return result.Success ? Ok(result) : BadRequest(result);
    }
    
    
    private Guid GetUserId()
        => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}

public record CreatePollRequest(
    Guid BuildingId,
    string Title,
    string? Description,
    PollAudience Audience,
    DateTime Deadline,
    List<string> Options);
    
public record VotePollRequest(Guid PollId, Guid OptionId);