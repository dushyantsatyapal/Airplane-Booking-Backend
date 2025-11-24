using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace AirplaneBooking.Infrastructure.Persistence;

    public class MongoDbContext
    {
        private readonly IMongoDatabase _database;
        private readonly ILogger<MongoDbContext> _logger;

        public MongoDbContext(IConfiguration configuration, ILogger<MongoDbContext> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            var connectionString = configuration["MongoDb:ConnectionString"];
            var databaseName = configuration["MongoDb:DatabaseName"];

            if (string.IsNullOrEmpty(connectionString))
            {
                _logger.LogError("MongoDB ConnectionString not found in configuration.");
                throw new InvalidOperationException("MongoDB ConnectionString not found in configuration.");
            }
            if (string.IsNullOrEmpty(databaseName))
            {
                _logger.LogError("MongoDB DatabaseName not found in configuration.");
                throw new InvalidOperationException("MongoDB DatabaseName not found in configuration.");
            }

            try
            {
                var client = new MongoClient(connectionString);
                _database = client.GetDatabase(databaseName);
                _logger.LogInformation($"Successfully connected to MongoDB database: {databaseName}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to connect to MongoDB. Check connection string and server availability.");
                throw new InvalidOperationException("Failed to connect to MongoDB. See inner exception for details.", ex);
            }
        }

        public IMongoCollection<T> GetCollection<T>(string name)
        {
            return _database.GetCollection<T>(name);
        }
    }
