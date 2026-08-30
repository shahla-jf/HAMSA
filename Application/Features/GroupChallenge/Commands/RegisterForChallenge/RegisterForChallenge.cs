using HAMSA.Domain.Entities;
using HAMSA.Domain.Enums;
using HAMSA.Domain.Interfaces.Repositories;

namespace HAMSA.Application.Features.GroupChallenge.Commands.RegisterForChallenge;

public record RegisterForChallengeCommand(
    Guid BuildingId,
    Guid UserId,
    string? FullName,
    int Age,
    Gender Gender,
    SportsBackground SportsBackground);

public record RegisterForChallengeResult(bool Success, string Message);

public class RegisterForChallengeHandler
{
    private readonly IGroupChallengeRepository _challengeRepository;
    private readonly IBuildingMembershipRepository _membershipRepository;
    private readonly IUserRepository _userRepository;

    public RegisterForChallengeHandler(
        IGroupChallengeRepository challengeRepository,
        IBuildingMembershipRepository membershipRepository,
        IUserRepository userRepository)
    {
        _challengeRepository = challengeRepository;
        _membershipRepository = membershipRepository;
        _userRepository = userRepository;
    }

    public async Task<RegisterForChallengeResult> HandleAsync(RegisterForChallengeCommand command)
    {
        var membership = await _membershipRepository.GetActiveAsync(command.UserId, command.BuildingId);
        if (membership is null)
            return new(false, "شما عضو این ساختمان نیستید.");

        if (command.Age <= 0 || command.Age > 120)
            return new(false, "سن نامعتبر است.");

        var challenge = await _challengeRepository.GetCurrentAsync(command.BuildingId);

        // اگر چالش فعلی وجود ندارد، یکی ایجاد می‌کنیم
        if (challenge is null)
        {
            challenge = CreateWeeklyChallenge(command.BuildingId);
            await _challengeRepository.AddAsync(challenge);
        }

        // فقط در بازه ثبت‌نام می‌توان ثبت‌نام کرد
        if (challenge.Status != GroupChallengeStatus.RegistrationOpen)
            return new(false, "بازه ثبت‌نام به پایان رسیده است.");

        var now = DateTime.UtcNow;
        if (now < challenge.RegistrationOpenDate || now > challenge.RegistrationCloseDate)
            return new(false, "بازه ثبت‌نام فعال نیست.");

        var isRegistered = await _challengeRepository.IsUserRegisteredAsync(challenge.Id, command.UserId);
        if (isRegistered)
            return new(false, "شما قبلاً در این چالش ثبت‌نام کرده‌اید.");

        // اگر نام خالی بود، از دیتابیس بخوان
        var fullName = command.FullName?.Trim();
        if (string.IsNullOrWhiteSpace(fullName))
        {
            var user = await _userRepository.GetByIdAsync(command.UserId);
            fullName = user is null ? "کاربر ناشناس" : $"{user.FirstName} {user.LastName}";
        }

        var participant = GroupChallengeParticipant.Create(
            challenge.Id,
            command.UserId,
            fullName,
            command.Age,
            command.Gender,
            command.SportsBackground.ToString());

        challenge.AddParticipant(participant);

        await _challengeRepository.SaveChangesAsync();

        return new(true, "ثبت‌نام شما با موفقیت انجام شد.");
    }

    private static Domain.Entities.GroupChallenge CreateWeeklyChallenge(Guid buildingId)
    {
        // محاسبه شنبه، دوشنبه و جمعه هفته جاری
        var today = DateTime.UtcNow.Date;
        var daysUntilSaturday = ((int)DayOfWeek.Saturday - (int)today.DayOfWeek + 7) % 7;
        if (daysUntilSaturday == 0 && today.DayOfWeek != DayOfWeek.Saturday)
            daysUntilSaturday = 7;

        // اگر الان شنبه، یکشنبه یا دوشنبه است، هفته جاری؛ در غیر این صورت هفته آینده
        var saturday = (int)today.DayOfWeek >= (int)DayOfWeek.Tuesday
            ? today.AddDays(daysUntilSaturday)
            : today.AddDays(-((int)today.DayOfWeek - (int)DayOfWeek.Saturday + 7) % 7);

        // ساده‌سازی: شنبه نزدیک بعدی یا همان امروز اگر شنبه بود
        var daysFromSaturday = ((int)today.DayOfWeek - (int)DayOfWeek.Saturday + 7) % 7;
        if (daysFromSaturday > 2) // سه‌شنبه به بعد
        {
            saturday = today.AddDays(7 - daysFromSaturday);
        }
        else
        {
            saturday = today.AddDays(-daysFromSaturday);
        }

        var monday = saturday.AddDays(2).AddHours(23).AddMinutes(59);
        var friday = saturday.AddDays(5).AddHours(23).AddMinutes(59);

        return Domain.Entities.GroupChallenge.Create(buildingId, saturday, monday, friday);
    }
}