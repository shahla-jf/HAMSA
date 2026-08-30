using HAMSA.Domain.Enums;

namespace HAMSA.Domain.Interfaces.Services;

public interface ISmsService
{
    Task SendOtpAsync(string phoneNumber, string code);
}

public interface IJwtService
{
    string GenerateToken(Guid userId, string phoneNumber);
    Guid? ValidateToken(string token);
}

public interface IFileStorageService
{
    Task<string> UploadImageAsync(
        Stream stream,
        string fileName,
        string contentType,
        CancellationToken cancellationToken = default);
}

public interface IGroupChallengeAIService
{
    Task<AiChallengeResponse> GenerateChallengeAsync(
        Guid buildingId,
        List<ParticipantProfile> participants,
        CancellationToken cancellationToken = default);
}

public record ParticipantProfile(
    int Age,
    Gender Gender,
    string SportsBackground);

public record AiChallengeResponse(
    string Title,
    string Description);