using HAMSA.Domain.Entities;
using HAMSA.Domain.Enums;
using HAMSA.Domain.Interfaces.Repositories;
using HAMSA.Domain.Interfaces.Services;

namespace HAMSA.Application.Features.GroupChallenge.Queries;

public record GetChallengeStatusQuery(Guid BuildingId, Guid UserId);

public enum ChallengePhase
{
    NoChallenge,           // هیچ چالشی فعال نیست
    RegistrationOpen,      // بازه ثبت‌نام
    ChallengeActive,       // بازه انجام چالش
    ChallengeCompleted     // چالش پایان یافته
}

public record ChallengeStatusDto(
    ChallengePhase Phase,
    Guid? ChallengeId,
    bool IsRegistered,
    bool HasCompleted,
    DateTime? RegistrationCloseDate,
    DateTime? ChallengeDeadline);

public record GetChallengeStatusResult(bool Success, string Message, ChallengeStatusDto? Data);

public class GetChallengeStatusHandler
{
    private readonly IGroupChallengeRepository _challengeRepository;

    public GetChallengeStatusHandler(IGroupChallengeRepository challengeRepository)
    {
        _challengeRepository = challengeRepository;
    }

    public async Task<GetChallengeStatusResult> HandleAsync(GetChallengeStatusQuery query)
    {
        var challenge = await _challengeRepository.GetCurrentAsync(query.BuildingId);

        if (challenge is null)
        {
            return new(true, "چالشی فعال نیست.", new ChallengeStatusDto(
                ChallengePhase.NoChallenge, null, false, false, null, null));
        }

        var now = DateTime.UtcNow;
        var isRegistered = challenge.Participants.Any(p => p.UserId == query.UserId);
        var participant = challenge.Participants.FirstOrDefault(p => p.UserId == query.UserId);
        var hasCompleted = participant?.IsCompleted ?? false;

        ChallengePhase phase;
        if (now < challenge.RegistrationCloseDate && challenge.Status == GroupChallengeStatus.RegistrationOpen)
            phase = ChallengePhase.RegistrationOpen;
        else if (now <= challenge.ChallengeDeadline && challenge.Status == GroupChallengeStatus.ChallengeActive)
            phase = ChallengePhase.ChallengeActive;
        else
            phase = ChallengePhase.ChallengeCompleted;

        return new(true, "وضعیت چالش دریافت شد.", new ChallengeStatusDto(
            phase,
            challenge.Id,
            isRegistered,
            hasCompleted,
            challenge.RegistrationCloseDate,
            challenge.ChallengeDeadline));
    }
}



public record GetChallengeDetailsQuery(Guid BuildingId, Guid UserId);

public record CompletedParticipantDto(string FullName, DateTime? CompletedAt);

public record ChallengeDetailsDto(
    Guid Id,
    string Title,
    string Description,
    DateTime Deadline,
    bool IsRegistered,
    bool HasCompleted,
    int TotalParticipants,
    int CompletedCount,
    List<CompletedParticipantDto> CompletedParticipants);

public record GetChallengeDetailsResult(bool Success, string Message, ChallengeDetailsDto? Data);

public class GetChallengeDetailsHandler
{
    private readonly IGroupChallengeRepository _challengeRepository;
    private readonly IGroupChallengeAIService _aiService;

    public GetChallengeDetailsHandler(
        IGroupChallengeRepository challengeRepository,
        IGroupChallengeAIService aiService)
    {
        _challengeRepository = challengeRepository;
        _aiService = aiService;
    }

    public async Task<GetChallengeDetailsResult> HandleAsync(GetChallengeDetailsQuery query)
    {
        var challenge = await _challengeRepository.GetCurrentWithParticipantsAsync(query.BuildingId);

        // اگر چالشی وجود ندارد
        if (challenge is null)
            return new(false, "چالشی فعال نیست.", null);

        // اگر چالش Active است ولی AI هنوز تولید نکرده، حالا تولید می‌کنیم
        if (challenge.Status == GroupChallengeStatus.ChallengeActive && string.IsNullOrWhiteSpace(challenge.Title))
        {
            if (challenge.Participants.Count == 0)
                return new(false, "هیچ شرکت‌کننده‌ای برای تولید چالش وجود ندارد.", null);

            var profiles = challenge.Participants.Select(p => new ParticipantProfile(
                p.Age,
                Enum.Parse<Gender>(p.Gender.ToString()),
                p.SportsBackground ?? "Intermediate"
            )).ToList();

            var aiResult = await _aiService.GenerateChallengeAsync(challenge.BuildingId, profiles);
            challenge.SetAiGeneratedChallenge(aiResult.Title, aiResult.Description);
            await _challengeRepository.SaveChangesAsync();
        }

        var now = DateTime.UtcNow;
        if (now < challenge.RegistrationCloseDate && challenge.Status == GroupChallengeStatus.RegistrationOpen)
            return new(false, "هنوز در بازه ثبت‌نام هستید. چالش بعد از پایان ثبت‌نام مشخص می‌شود.", null);

        var isRegistered = challenge.Participants.Any(p => p.UserId == query.UserId);
        var hasCompleted = challenge.Participants
            .FirstOrDefault(p => p.UserId == query.UserId)?.IsCompleted ?? false;

        var completedParticipants = challenge.Participants
            .Where(p => p.IsCompleted)
            .Select(p => new CompletedParticipantDto(p.FullName, p.CompletedAt))
            .ToList();

        return new(true, "جزئیات چالش دریافت شد.", new ChallengeDetailsDto(
            challenge.Id,
            challenge.Title ?? "چالش در حال تولید...",
            challenge.Description ?? "",
            challenge.ChallengeDeadline,
            isRegistered,
            hasCompleted,
            challenge.Participants.Count,
            completedParticipants.Count,
            completedParticipants));
    }
}