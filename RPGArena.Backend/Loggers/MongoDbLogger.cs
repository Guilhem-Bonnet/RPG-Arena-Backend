// RPGArena.Backend/Loggers/MongoDbLogger.cs
using ILogger = RPGArena.CombatEngine.Logging.ILogger;
using MongoDB.Bson;
using MongoDB.Driver;
using RPGArena.CombatEngine.Logging;
/*
*<summary>
*   Logger persistant qui enregistre les logs du combat dans une base MongoDB.
*   Utile pour audit, historique, analyse statistique ou visualisation future des combats.
*</summary>
*/
namespace RPGArena.Backend.Loggers;

public class MongoDbLogger : ILogger
{
    private readonly IMongoCollection<BsonDocument> _collection;

    public MongoDbLogger(IMongoClient mongoClient)
    {
        var database = mongoClient.GetDatabase("RPGArena");
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