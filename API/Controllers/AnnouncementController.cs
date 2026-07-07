using HAMSA.Application.Features.Announcements.Commands.CreateAnnouncement;
using HAMSA.Application.Features.Announcements.Commands.MarkAnnouncementRead;
using HAMSA.Application.Features.Announcements.Queries;
using HAMSA.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using HAMSA.Application.Features.Announcements.Commands.DeleteAnnouncement;

namespace HAMSA.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AnnouncementController : ControllerBase
{
    private readonly CreateAnnouncementHandler _createHandler;
    private readonly MarkAnnouncementReadHandler _markReadHandler;
    private readonly GetAnnouncementsHandler _getAnnouncementsHandler;
    private readonly DeleteAnnouncementHandler _deleteHandler;

    public AnnouncementController(
        CreateAnnouncementHandler createHandler,
        MarkAnnouncementReadHandler markReadHandler,
        GetAnnouncementsHandler getAnnouncementsHandler,
        DeleteAnnouncementHandler deleteHandler)
    {
        _createHandler = createHandler;
        _markReadHandler = markReadHandler;
        _getAnnouncementsHandler = getAnnouncementsHandler;
        _deleteHandler = deleteHandler;
    }

    /// <summary>
    /// لیست اطلاعیه‌های ساختمان
    /// </summary>
    [HttpGet("{buildingId}")]
    public async Task<IActionResult> GetAnnouncements(Guid buildingId)
    {
        var userId = GetUserId();

        var result = await _getAnnouncementsHandler.HandleAsync(
            new GetAnnouncementsQuery(buildingId, userId));

        return Ok(result);
    }

    /// <summary>
    /// ثبت اطلاعیه (فقط مدیر)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create(CreateAnnouncementRequest request)
    {
        var userId = GetUserId();

        var result = await _createHandler.HandleAsync(
            new CreateAnnouncementCommand(
                request.BuildingId,
                userId,
                request.Title,
                request.Description,
                request.Priority));

        return result.Success
            ? Ok(result)
            : BadRequest(result);
    }

    /// <summary>
    /// علامت‌گذاری به عنوان خوانده شده
    /// </summary>
    [HttpPost("{announcementId}/read")]
    public async Task<IActionResult> MarkRead(Guid announcementId)
    {
        var userId = GetUserId();

        var result = await _markReadHandler.HandleAsync(
            new MarkAnnouncementReadCommand(
                announcementId,
                userId));

        return result.Success
            ? Ok(result)
            : BadRequest(result);
    }

    /// <summary>
    /// حذف اطلاعیه (فقط مدیر)
    /// </summary>
    [HttpDelete("{announcementId}")]
    public async Task<IActionResult> Delete(Guid announcementId)
    {
        var userId = GetUserId();

        var result = await _deleteHandler.HandleAsync(
            new DeleteAnnouncementCommand(
                announcementId,
                userId));

        return result.Success
            ? Ok(result)
            : BadRequest(result);
    }

    private Guid GetUserId()
        => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}

public record CreateAnnouncementRequest(
    Guid BuildingId,
    string Title,
    string Description,
    AnnouncementPriority Priority);