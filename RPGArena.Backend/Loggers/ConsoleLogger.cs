using ILogger = RPGArena.CombatEngine.Logging.ILogger;
using RPGArena.CombatEngine.Logging;
/*
<summary>
    Logger console utilisé dans le contexte du serveur ASP.NET. Sert au suivi du backend WebSocket.
    Peut différer du logger console du moteur si des contraintes de contexte sont nécessaires.
</summary>
   
*/
namespace RPGArena.Backend.Loggers
{
    public class ConsoleLogger : ILogger
    {
        public void Log(string message)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"[ConsoleLogger] {message}");
            Console.ResetColor();
        }
    }
}
