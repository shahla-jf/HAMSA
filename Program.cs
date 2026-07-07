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
using HAMSA.Application.Features.Buildings.Commands.CreateBuilding;
using HAMSA.Application.Features.Buildings.Commands.TransferManager;
using HAMSA.Application.Features.Buildings.Commands.UpdateBuilding;
using HAMSA.Application.Features.Buildings.Queries;
using HAMSA.Application.Features.Units.Commands.AddOwner;
using HAMSA.Application.Features.Units.Commands.AddTenant;
using HAMSA.Application.Features.Units.Queries;

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
            .WithOrigins("https://yourfrontend.com")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

builder.Services.AddScoped<IOtpRepository, OtpRepository>();
builder.Services.AddScoped<ISmsService, SmsService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<SendOtpHandler>();
builder.Services.AddScoped<VerifyOtpHandler>();
builder.Services.AddScoped<CreateBuildingHandler>();
builder.Services.AddScoped<UpdateBuildingHandler>();
builder.Services.AddScoped<TransferManagerHandler>();
builder.Services.AddScoped<GetBuildingHandler>();
builder.Services.AddScoped<GetUserBuildingsHandler>();
builder.Services.AddScoped<AddOwnerHandler>();
builder.Services.AddScoped<AddTenantHandler>();
builder.Services.AddScoped<GetUnitsByBuildingHandler>();
builder.Services.AddScoped<GetMyTenantsHandler>();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();


app.UseSwagger();
app.UseSwaggerUI();


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
