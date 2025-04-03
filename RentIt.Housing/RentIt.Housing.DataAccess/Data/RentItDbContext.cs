using MongoDB.Driver;

namespace RentIt.Housing.DataAccess.Data
{
    public class RentItDbContext
    {
        private readonly IMongoClient _mongoClient;
        private readonly IMongoDatabase? _database;

        public RentItDbContext(IMongoClient mongoClient)
        {
            _mongoClient = mongoClient;
            _database = _mongoClient.GetDatabase("housing_db");
        }

        public IMongoDatabase? Database => _database;

        public IMongoCollection<TEntity> Set<TEntity>(string? collectionName = null)
                where TEntity : class
        {
            if (_database == null)
            {
                throw new InvalidOperationException("База данных не инициализирована.");
            }

            collectionName ??= typeof(TEntity).Name;

            return _database.GetCollection<TEntity>(collectionName);
        }
    }
}