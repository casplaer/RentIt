using Microsoft.EntityFrameworkCore;
using RentIt.Bookings.API.Extensions;
using RentIt.Bookings.Application.Extensions;
using RentIt.Bookings.Infrastructure.Data;
using RentIt.Bookings.Infrastructure.Extensions;
using RentIt.Protos.Housing;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("BookingDatabaseConnection");

var configuration = builder.Configuration;

builder.Services.AddDbContext<RentItDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddLogging(configuration);

builder.Host.UseSerilog();

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddRedis(configuration);
builder.Services.AddJwtAuthentication(configuration);

builder.Services.AddApplicationServices();
builder.Services.AddApplicationUseCases();
builder.Services.AddApplicationRepositories();

builder.Services.AddGrpcClient<HousingService.HousingServiceClient>(options =>
{
    options.Address = new Uri("https://localhost:7175");
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCustomMiddlewares();

app.UseHttpsRedirection();

app.UseHttpsRedirection();
app.MapControllers();

app.Run();