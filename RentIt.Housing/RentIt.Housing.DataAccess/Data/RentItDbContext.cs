using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace RentIt.Housing.DataAccess.Data
{
    public class RentItDbContext
    {
        private readonly IConfiguration _configuration;
        private readonly IMongoDatabase? _database;

        public RentItDbContext(IConfiguration configuration)
        {
            _configuration = configuration;

            var connectionString = _configuration.GetConnectionString("HousingDatabaseConnection");
            var mongoUrl = MongoUrl.Create(connectionString);
            var mongoClient = new MongoClient(mongoUrl);
            _database = mongoClient.GetDatabase(mongoUrl.DatabaseName);
        }

        public IMongoCollection<TEntity> Set<TEntity>(string? collectionName = null)
                where TEntity : class
        {
            if (_database == null)
            {
                throw new InvalidOperationException("Database is not initialized.");
            }

            collectionName ??= typeof(TEntity).Name;

            return _database.GetCollection<TEntity>(collectionName);
        }

        public IMongoDatabase? Database => _database;
    }
}