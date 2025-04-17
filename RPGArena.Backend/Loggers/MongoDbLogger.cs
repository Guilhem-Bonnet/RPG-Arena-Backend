// RPGArena.Backend/Loggers/MongoDbLogger.cs
using ILogger = RPGArena.CombatEngine.Logging.ILogger;
using MongoDB.Bson;
using MongoDB.Driver;
using RPGArena.CombatEngine.Logging;

namespace RPGArena.Backend.Loggers;

public class MongoDbLogger : ILogger
{
    private readonly IMongoCollection<BsonDocument> _collection;

    public MongoDbLogger()
    {
        var client = new MongoClient("mongodb://localhost:27017"); // à adapter si besoin WIP
        var database = client.GetDatabase("rpgarena");
        _collection = database.GetCollection<BsonDocument>("combat_logs");
    }

    public void Log(string message)
    {
        var doc = new BsonDocument
        {
            { "timestamp", BsonDateTime.Create(DateTime.UtcNow) },
            { "message", message }
        };

        _collection.InsertOne(doc);
    }
}