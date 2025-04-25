using Microsoft.EntityFrameworkCore;
using RentIt.Users.API.Endpoints;
using RentIt.Users.API.Extensions;
using RentIt.Users.Infrastructure.Data;
using Microsoft.AspNetCore.Http.Json;
using System.Text.Json.Serialization;
using RentIt.Users.Infrastructure.Options;
using RentIt.Users.Application.Extensions;
using RentIt.Users.Infrastructure.Extensions;
using Hangfire;
using RentIt.Users.Infrastructure.Services.Grpc;
using RentIt.Users.Infrastructure.Services;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("UsersDatabaseConnection");

var configuration = builder.Configuration;

builder.Services.AddLogging(configuration);

builder.Host.UseSerilog();

builder.Services.AddDbContext<RentItDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddGrpc();
builder.Services.AddApplicationRepositories();
builder.Services.AddApplicationUtilities();
builder.Services.AddMediatR();
builder.Services.MapAllProfiles();
builder.Services.AddValidators();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.ReferenceHandler = ReferenceHandler.Preserve;
});

builder.Services.Configure<SmtpOptions>(configuration.GetSection("Smtp"));
builder.Services.Configure<JwtOptions>(configuration.GetSection("Jwt"));

builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

builder.Services.AddRedis(configuration);
builder.Services.AddUsersHangfire(configuration);
builder.Services.AddJwtAuthentication(configuration);

builder.Services.AddApplicationServices();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<RentItDbContext>();
    dbContext.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.UseCustomMiddlewares();
app.UseHangfireDashboard("/hangfire");

HangfireJobsService.ConfigureHangfireJobs(app);

app.MapGrpcService<UsersGrpcService>();

app.UseHttpsRedirection();

app.MapUserEndpoints();

app.Run();
