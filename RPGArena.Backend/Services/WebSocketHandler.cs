using RPGArena.Backend.Loggers;
using ILogger = RPGArena.CombatEngine.Logging.ILogger;
using System.Net.WebSockets;
using System.Text.Json;
using System.Text;
using RPGArena.CombatEngine.Core;
using RPGArena.CombatEngine.Logging;

public class WebSocketHandler
{
    private readonly WebSocketLoggerFactory _loggerFactory;
    private readonly ConsoleLogger _consoleLogger;
    private readonly MongoDbLogger _mongoLogger;

    public WebSocketHandler(WebSocketLoggerFactory loggerFactory, ConsoleLogger consoleLogger, MongoDbLogger mongoLogger)
    {
        _loggerFactory = loggerFactory;
        _consoleLogger = consoleLogger;
        _mongoLogger = mongoLogger;
    }

    public async Task HandleConnection(WebSocket socket)
    {
        var buffer = new byte[1024];
        var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
        var json = Encoding.UTF8.GetString(buffer, 0, result.Count);

        var characters = JsonSerializer.Deserialize<List<string>>(json);

        if (characters == null || characters.Count < 2)
        {
            await socket.CloseAsync(WebSocketCloseStatus.InvalidPayloadData, "Invalid data", CancellationToken.None);
            return;
        }

        var logger = new MultiLogger(new ILogger[]
        {
            _loggerFactory.CreateLogger(socket),
            _consoleLogger,
            _mongoLogger
        });


        var arena = new BattleArena(characters, logger);
        await arena.StartBattle();

        await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Battle ended", CancellationToken.None);
    }
}
