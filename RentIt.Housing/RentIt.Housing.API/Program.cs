using RentIt.Housing.DataAccess.Data;
using RentIt.Housing.Domain.Extensions;
using RentIt.Housing.DataAccess.Extensions;
using RentIt.Housing.API.Extensions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using RentIt.Protos.Users;
using Hangfire;
using Serilog;
using MongoDB.Driver;
using RentIt.Housing.Domain.Services;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("HousingDatabaseConnection");

BsonSerializer.RegisterSerializer(typeof(Guid), new GuidSerializer(GuidRepresentation.Standard));

var configuration = builder.Configuration;

builder.Services.AddLogging(configuration);

builder.Host.UseSerilog();

builder.Services.AddControllers();

builder.Services.AddSingleton<IMongoClient>(options =>
{
    var mongoUrl = MongoUrl.Create(connectionString);
    return new MongoClient(mongoUrl);
});

builder.Services.AddSingleton<RentItDbContext>();

builder.Services.AddRepositories();
builder.Services.AddDomainServices();
builder.Services.AddMappingProfiles();
builder.Services.AddValidators();

builder.Services.AddRedis(configuration);
builder.Services.AddJwtAuthentication(configuration);
builder.Services.AddHousingHangfire(configuration);

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
HangfireJobsService.ConfigureRecurringJobs();

app.UseHttpsRedirection();
app.MapControllers();

app.Run();