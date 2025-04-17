using System.Net.WebSockets;
using RPGArena.CombatEngine.Logging;
using ILogger = RPGArena.CombatEngine.Logging.ILogger;

namespace RPGArena.Backend.Loggers;

public class WebSocketLoggerFactory
{
    public ILogger CreateLogger(WebSocket socket)
    {
        return new WebSocketLogger(socket);
    }
}
