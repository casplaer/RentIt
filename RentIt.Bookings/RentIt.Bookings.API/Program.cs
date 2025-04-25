using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RentIt.Bookings.API.Extensions;
using RentIt.Bookings.Application.Extensions;
using RentIt.Bookings.Infrastructure.Data;
using RentIt.Bookings.Infrastructure.Extensions;
using RentIt.Bookings.Infrastructure.Options;
using RentIt.Bookings.Infrastructure.Services;
using RentIt.Bookings.Infrastructure.Services.Grpc;
using RentIt.Protos.Housing;
using RentIt.Protos.Users;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("BookingDatabaseConnection");

var configuration = builder.Configuration;

builder.Services.AddDbContext<RentItDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddLogging(configuration);

builder.Host.UseSerilog();

builder.Services.AddGrpc();

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddRedis(configuration);
builder.Services.AddBookingsHangfire(configuration);
builder.Services.AddJwtAuthentication(configuration);

builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices();
builder.Services.AddApplicationUseCases();
builder.Services.AddApplicationRepositories();

builder.Services.Configure<MessageBrokerOptions>(
    configuration.GetSection("MessageBroker"));

builder.Services.AddSingleton(sp =>
    sp.GetRequiredService<IOptions<MessageBrokerOptions>>().Value);
builder.Services.Configure<SmtpOptions>(builder.Configuration.GetSection("Smtp"));

builder.Services.AddRabbitMq();

builder.Services.AddGrpcClient<HousingService.HousingServiceClient>(options =>
{
    options.Address = new Uri("https://localhost:7175");
});

builder.Services.AddGrpcClient<UsersService.UsersServiceClient>(options =>
{
    options.Address = new Uri("https://localhost:7108");
});

builder.Services.MapAllProfiles();
builder.Services.AddValidators();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCustomMiddlewares();

app.MapGrpcService<BookingsGrpcService>();

app.UseHttpsRedirection();

app.UseHttpsRedirection();
app.MapControllers();

app.UseHangfireDashboard("/hangfire");
HangfireJobsService.ConfigureHangfireJobs(app);

app.Run();