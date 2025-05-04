using System.Net.WebSockets;
using RPGArena.CombatEngine.Logging;
using ILogger = RPGArena.CombatEngine.Logging.ILogger;

namespace RPGArena.Backend.Loggers;
/// <summary>
/// WebSocketLoggerFactory permet de créer dynamiquement un logger WebSocket
/// pour une session WebSocket donnée, afin que les messages du combat soient directement
/// transmis au client connecté.
/// </summary>

public class WebSocketLoggerFactory
{
    public ILogger CreateLogger(WebSocket socket)
    {
        return new WebSocketLogger(socket);
    }
}
