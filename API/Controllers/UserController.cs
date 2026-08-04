using System.Security.Claims;
using HAMSA.Application.Features.User.Commands.UpdateProfile;
using HAMSA.Application.Features.User.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HAMSA.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]

public class UserController : ControllerBase
{
    private readonly GetProfileHandler _getProfileHandler;
    private readonly UpdateProfileHandler _updateProfileHandler;

    public UserController(
        GetProfileHandler getProfileHandler,
        UpdateProfileHandler updateProfileHandler)
    {
        _getProfileHandler = getProfileHandler;
        _updateProfileHandler = updateProfileHandler;
    }

    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        var userId = GetUserId();
        var result = await _updateProfileHandler.HandleAsync(new UpdateProfileCommand(
            userId, request.FirstName, request.LastName));
        return Ok(result);
    }

    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        var userId = GetUserId();
        var result = await _getProfileHandler.HandleAsync(new GetProfileQuery(userId));
        return result.Success ? Ok(result) : BadRequest(result);
    }
    
    private Guid GetUserId()
        => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}

public record UpdateProfileRequest(string? FirstName, string? LastName);