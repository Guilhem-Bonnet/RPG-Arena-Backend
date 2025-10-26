using System.Net.WebSockets;
using System.Text;
using RPGArena.CombatEngine.Logging;
using ILogger = RPGArena.CombatEngine.Logging.ILogger;

namespace RPGArena.Backend.Loggers;

public class WebSocketLogger : ILogger
{
    private readonly WebSocket _socket;

    public WebSocketLogger(WebSocket socket)
    {
        _socket = socket;
    }

    public void Log(string message)
    {
        if (_socket.State == WebSocketState.Open)
        {
            // Fire-and-forget asynchrone pour éviter les deadlocks
            _ = Task.Run(async () =>
            {
                try
                {
                    var bytes = Encoding.UTF8.GetBytes(message + "\n");
                    await _socket.SendAsync(bytes, WebSocketMessageType.Text, true, CancellationToken.None);
                }
                catch (Exception ex)
                {
                    // Log silencieux des erreurs pour ne pas bloquer le combat
                    Console.WriteLine($"⚠️ WebSocket send error: {ex.Message}");
                }
            });
        }
    }
}
