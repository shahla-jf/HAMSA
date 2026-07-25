using HAMSA.Infrastructure.DependencyInjection;
using HAMSA.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using HAMSA.Application.Features.Auth.Commands.SendOtp;
using HAMSA.Application.Features.Auth.Commands.VerifyOtp;
using HAMSA.Domain.Interfaces.Repositories;
using HAMSA.Domain.Interfaces.Services;
using HAMSA.Infrastructure.ExternalServices;
using HAMSA.Infrastructure.Persistence.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using HAMSA.Application.Features.Announcements.Commands.CreateAnnouncement;
using HAMSA.Application.Features.Announcements.Commands.DeleteAnnouncement;
using HAMSA.Application.Features.Announcements.Commands.MarkAnnouncementRead;
using HAMSA.Application.Features.Announcements.Queries;
using HAMSA.Application.Features.Buildings.Commands.CreateBuilding;
using HAMSA.Application.Features.Buildings.Commands.SelectCurrentBuilding;
using HAMSA.Application.Features.Buildings.Commands.TransferManager;
using HAMSA.Application.Features.Buildings.Commands.UpdateBuilding;
using HAMSA.Application.Features.Buildings.Queries;
using HAMSA.Application.Features.Polls.Commands.VotePoll;
using HAMSA.Application.Features.Reports.Commands.CreateRepairReport;
using HAMSA.Application.Features.Reports.Commands.UpdateRepairStatus;
using HAMSA.Application.Features.Reports.Queries;
using HAMSA.Application.Features.Reservations.Commands.CreateReservation;
using HAMSA.Application.Features.Reservations.Queries;
using HAMSA.Application.Features.Units.Commands.AddOwner;
using HAMSA.Application.Features.Units.Commands.AddTenant;
using HAMSA.Application.Features.Units.Commands.RemoveOneOwner;
using HAMSA.Application.Features.Units.Commands.RemoveOneTenant;
using HAMSA.Application.Features.Units.Commands.RemoveOwner;
using HAMSA.Application.Features.Units.Commands.RemoveTenant;
using HAMSA.Application.Features.Units.Queries;
using HAMSA.Application.Features.User.Commands.UpdateProfile;
using HAMSA.Application.Features.User.Queries;
using Scalar.AspNetCore;

DotNetEnv.Env.Load();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Configuration.AddEnvironmentVariables();

var connectionString =
    Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
    ?? builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Database connection string not found.");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(
        connectionString,
        ServerVersion.AutoDetect(connectionString)
    )
);
builder.Services.AddInfrastructure();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateLifetime = true
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddHttpClient();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddScoped<IOtpRepository, OtpRepository>();
builder.Services.AddScoped<ISmsService, SmsService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<SendOtpHandler>();
builder.Services.AddScoped<VerifyOtpHandler>();
builder.Services.AddScoped<UpdateProfileHandler>();
builder.Services.AddScoped<GetProfileHandler>();
builder.Services.AddScoped<CreateBuildingHandler>();
builder.Services.AddScoped<IFileStorageService, FileStorageService>();
builder.Services.AddScoped<UpdateBuildingHandler>();
builder.Services.AddScoped<TransferManagerHandler>();
builder.Services.AddScoped<GetBuildingHandler>();
builder.Services.AddScoped<GetUserBuildingsHandler>();
builder.Services.AddScoped<AddOwnerHandler>();
builder.Services.AddScoped<AddTenantHandler>();
builder.Services.AddScoped<RemoveOneOwnerHandler>();
builder.Services.AddScoped<RemoveOneTenantHandler>();
builder.Services.AddScoped<RemoveOwnerHandler>();
builder.Services.AddScoped<RemoveTenantHandler>();
builder.Services.AddScoped<GetUnitsByBuildingHandler>();
builder.Services.AddScoped<GetMyTenantsHandler>();
builder.Services.AddScoped<CreateAnnouncementHandler>();
builder.Services.AddScoped<GetAnnouncementsHandler>(); 
builder.Services.AddScoped<DeleteAnnouncementHandler>();
builder.Services.AddScoped<MarkAnnouncementReadHandler>();
builder.Services.AddScoped<CreateRepairReportHandler>();
builder.Services.AddScoped<UpdateRepairStatusHandler>();
builder.Services.AddScoped<GetRepairReportsHandler>();
builder.Services.AddScoped<CreateReservationHandler>();
builder.Services.AddScoped<GetReservedDatesHandler>();
builder.Services.AddScoped<GetMyReservationsHandler>();
builder.Services.AddScoped<GetMyRoleInBuildingHandler>();
builder.Services.AddScoped<CreatePollHandler>();
builder.Services.AddScoped<VotePollHandler>();
builder.Services.AddScoped<GetActivePollsHandler>();
builder.Services.AddScoped<GetInActivePollsHandler>();
builder.Services.AddScoped<GetMyPollVoteHandler>();
builder.Services.AddScoped<SelectCurrentBuildingHandler>();
builder.Services.AddScoped<GetLastSelectedBuildingHandler>();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseCors("AllowFrontend");

app.MapScalarApiReference(options =>
{
    options.WithOpenApiRoutePattern("/swagger/v1/swagger.json");
});

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();


app.UseSwagger();


using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();

    try
    {
        var db = services.GetRequiredService<AppDbContext>();
        db.Database.Migrate();
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while applying migrations.");
        throw;
    }
}

app.Run();
