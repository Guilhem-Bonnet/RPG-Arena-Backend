using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using RPGArena.Backend.Services;
using RPGArena.CombatEngine.Logging;
using ILogger = RPGArena.CombatEngine.Logging.ILogger;
using ConsoleLogger = RPGArena.CombatEngine.Logging.ConsoleLogger;
using RPGArena.Backend.Loggers;

namespace RPGArena.Backend.Services;

public class CharacterConfigDto
{
    public string type { get; set; } = "guerrier";
    public string name { get; set; } = "";
}

public class WebSocketHandler
{
    private readonly WebSocketLoggerFactory _loggerFactory;
    private readonly ConsoleLogger _consoleLogger;
    private readonly MongoDbLogger _mongoLogger;
    private readonly BattleManager _battleManager;

    public WebSocketHandler(
        WebSocketLoggerFactory loggerFactory,
        ConsoleLogger consoleLogger,
        MongoDbLogger mongoLogger,
        BattleManager battleManager)
    {
        _loggerFactory = loggerFactory;
        _consoleLogger = consoleLogger;
        _mongoLogger = mongoLogger;
        _battleManager = battleManager;
    }

    public async Task HandleConnection(WebSocket socket)
    {
        var buffer = new byte[1024];
        var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
        var json = Encoding.UTF8.GetString(buffer, 0, result.Count);

        Console.WriteLine($"📨 Message reçu: {json}");

        // Essayer de désérialiser comme liste d'objets avec type et name
        List<(string Type, string Name)> characters;
        
        try
        {
            var configs = JsonSerializer.Deserialize<List<CharacterConfigDto>>(json);
            if (configs != null && configs.Count >= 2)
            {
                characters = configs.Select(c => (c.type, c.name)).ToList();
                Console.WriteLine($"✅ Format nouveau protocole détecté: {configs.Count} personnages");
            }
            else
            {
                // Essayer l'ancien format (liste de noms simples)
                var names = JsonSerializer.Deserialize<List<string>>(json);
                if (names == null || names.Count < 2)
                {
                    await socket.CloseAsync(WebSocketCloseStatus.InvalidPayloadData, "Invalid data: need at least 2 characters", CancellationToken.None);
                    return;
                }
                characters = names.Select(n => ("guerrier", n)).ToList();
                Console.WriteLine($"⚠️  Ancien format détecté, utilisation de 'guerrier' par défaut");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Erreur de désérialisation: {ex.Message}");
            await socket.CloseAsync(WebSocketCloseStatus.InvalidPayloadData, $"JSON error: {ex.Message}", CancellationToken.None);
            return;
        }

        var logger = new MultiLogger(new ILogger[]
        {
            _loggerFactory.CreateLogger(socket),
            _consoleLogger,
            _mongoLogger
        });

        await _battleManager.RunBattleAsync(characters, logger);
        await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Battle ended", CancellationToken.None);
    }
}
