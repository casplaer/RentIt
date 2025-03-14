using RentIt.Housing.DataAccess.Data;
using RentIt.Housing.Domain.Extensions;
using RentIt.Housing.DataAccess.Extensions;
using RentIt.Housing.API.Extensions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

var builder = WebApplication.CreateBuilder(args);

BsonSerializer.RegisterSerializer(typeof(Guid), new GuidSerializer(GuidRepresentation.Standard));

builder.Services.AddControllers();

builder.Services.AddSingleton<RentItDbContext>();

builder.Services.AddRepositories();
builder.Services.AddDomainServices();
builder.Services.AddMappingProfiles();
builder.Services.AddValidators();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCustomMiddlewares();

app.UseHttpsRedirection();
app.MapControllers();

app.Run();

