using backend;
using backend.Business.Services;
using backend.DataAccess.DbContext;
using backend.DataAccess.Repositories;
using backend.DataAccess.Seeding;
using backend.DataAccess.UnitOfWork;
using backend.Hubs;
using backend.Infrastructure.Security;
using backend.Infrastructure.Services;
using backend.Presentation;
using backend.Presentation.Handlers;
using Microsoft.AspNetCore.Builder;
using Newtonsoft.Json;

var builder = WebApplication.CreateBuilder(args);

// Configure Newtonsoft.Json for UTC timezone handling
JsonConvert.DefaultSettings = () => new JsonSerializerSettings
{
	DateTimeZoneHandling = DateTimeZoneHandling.Utc, DateFormatHandling = DateFormatHandling.IsoDateFormat
};

builder.Services.AddSingleton<DapperContext>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ITrainRepository, TrainRepository>();
builder.Services.AddScoped<ISeatRepository, SeatRepository>();
builder.Services.AddScoped<IBookingRepository, BookingRepository>();
builder.Services.AddScoped<IAuditLogRepository, AuditLogRepository>();

builder.Services.AddSingleton<PasswordHasher>();
builder.Services.AddSingleton<SessionManager>();

builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
builder.Services.AddScoped<ITrainService, TrainService>();
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<IUserService, UserService>();

builder.Services.AddScoped<DatabaseSeeder>();

builder.Services.AddTransient<AuthenticationHandler>();
builder.Services.AddTransient<TrainHandler>();
builder.Services.AddTransient<BookingHandler>();
builder.Services.AddTransient<UserHandler>();
builder.Services.AddTransient<AuditHandler>();

builder.Services.AddSingleton<TcpServer>();

builder.Services.AddSignalR();

builder.Services.AddHostedService<Worker>();
builder.Services.AddHostedService<SeatHoldCleanupService>();

var app = builder.Build();

var signalRHost = builder.Configuration.GetValue("SignalR:Host", "localhost");
var signalRPort = builder.Configuration.GetValue("SignalR:Port", 5001);
app.Urls.Add($"http://{signalRHost}:{signalRPort}");

app.MapHub<BookingHub>("/bookingHub");

var enableSeeding = builder.Configuration.GetValue<bool>("Database:EnableSeeding");
if (enableSeeding)
{
	using var scope = app.Services.CreateScope();
	var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
	await seeder.SeedAsync();
}

await app.RunAsync();