using RentIt.Housing.DataAccess.Data;
using RentIt.Housing.Domain.Extensions;
using RentIt.Housing.DataAccess.Extensions;
using RentIt.Housing.API.Extensions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using RentIt.Protos.Users;
using Hangfire;

var builder = WebApplication.CreateBuilder(args);

BsonSerializer.RegisterSerializer(typeof(Guid), new GuidSerializer(GuidRepresentation.Standard));

builder.Services.AddControllers();

builder.Services.AddSingleton<RentItDbContext>();

builder.Services.AddRepositories();
builder.Services.AddDomainServices();
builder.Services.AddMappingProfiles();
builder.Services.AddValidators();

builder.Services.AddRedis(builder.Configuration);
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddHousingHangfire(builder.Configuration);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddGrpcClient<UsersService.UsersServiceClient>(options =>
{
    options.Address = new Uri("https://localhost:7108");
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCustomMiddlewares();

app.UseHangfireDashboard("/hangfire");
HangfireJobsExtensions.ConfigureRecurringJobs();

app.UseHttpsRedirection();
app.MapControllers();

app.Run();

