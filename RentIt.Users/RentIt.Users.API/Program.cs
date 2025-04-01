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

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("UsersDatabaseConnection");

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

builder.Services.Configure<SmtpOptions>(builder.Configuration.GetSection("Smtp"));
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));

builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

builder.Services.AddRedis(builder.Configuration);
builder.Services.AddUsersHangfire(builder.Configuration);
builder.Services.AddJwtAuthentication(builder.Configuration);

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

HangfireJobsExtensions.ConfigureRecurringJobs();

app.MapGrpcService<UsersGrpcService>();

app.UseHttpsRedirection();

app.MapUserEndpoints();

app.Run();
