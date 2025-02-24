using Microsoft.EntityFrameworkCore;
using RentIt.Users.API.Endpoints;
using RentIt.Users.API.Extensions;
using RentIt.Users.Infrastructure.Data;
using Microsoft.AspNetCore.Http.Json;
using System.Text.Json.Serialization;
using RentIt.Users.Application.Options;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("UsersDatabaseConnection");

builder.Services.AddDbContext<RentItDbContext>(options =>
    options.UseNpgsql(connectionString));


builder.Services.AddApplicationRepositories();
builder.Services.AddApplicationUtilities();
builder.Services.AddCoreServices();
builder.Services.MapAllProfiles();
builder.Services.AddValidators();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.ReferenceHandler = ReferenceHandler.Preserve;
});

builder.Services.AddJwtAuthentication(builder.Configuration);

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
    await seeder.SeedAsync(CancellationToken.None);
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.UseCustomMiddlewares();

app.UseHttpsRedirection();

app.MapUserEndpoints();

app.Run();
