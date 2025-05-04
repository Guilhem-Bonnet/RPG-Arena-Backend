using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using RPGArena.Backend.Services;
using RPGArena.CombatEngine.Logging;
using ILogger = RPGArena.CombatEngine.Logging.ILogger;
using ConsoleLogger = RPGArena.CombatEngine.Logging.ConsoleLogger;
using RPGArena.Backend.Loggers;

namespace RPGArena.Backend.Services;

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

        var names = JsonSerializer.Deserialize<List<string>>(json);

        if (names == null || names.Count < 2)
        {
            await socket.CloseAsync(WebSocketCloseStatus.InvalidPayloadData, "Invalid data", CancellationToken.None);
            return;
        }

        // Convertir en paires (Type, Name) — tu peux remplacer "Default" si tu veux plus de flexibilité
        var characters = names.Select(n => ("Default", n)).ToList();

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