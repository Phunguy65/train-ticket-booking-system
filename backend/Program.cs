using backend;
using backend.Business.Services;
using backend.DataAccess.DbContext;
using backend.DataAccess.Repositories;
using backend.DataAccess.UnitOfWork;
using backend.Infrastructure.Security;
using backend.Presentation;
using backend.Presentation.Handlers;

var builder = Host.CreateApplicationBuilder(args);

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

builder.Services.AddTransient<AuthenticationHandler>();
builder.Services.AddTransient<TrainHandler>();
builder.Services.AddTransient<BookingHandler>();
builder.Services.AddTransient<UserHandler>();
builder.Services.AddTransient<AuditHandler>();

builder.Services.AddSingleton<TcpServer>();

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
