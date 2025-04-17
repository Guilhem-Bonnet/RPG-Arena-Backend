using ILogger = RPGArena.CombatEngine.Logging.ILogger;
using RPGArena.CombatEngine.Logging;

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
