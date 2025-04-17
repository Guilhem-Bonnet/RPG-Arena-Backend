using System;
using RPGArena.CombatEngine.Logging;

namespace RPGArena.CombatEngine.Logging
{
    public class ConsoleLogger : ILogger
    {
        public void Log(string message)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine($"[LOG] {message}");
            Console.ResetColor();
        }
    }
}
