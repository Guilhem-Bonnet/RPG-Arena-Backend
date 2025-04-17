using RPGArena.CombatEngine.Observeur;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGArena.CombatEngine.helper
{
    public class Logger
    {
        private static readonly Lazy<Logger> _instance = new Lazy<Logger>(() => new Logger());
        private readonly List<string> _logMessages = new List<string>(); // Déclaration de _logMessages

        public static Logger Instance => _instance.Value;

        public void Log(Message message)
        {
            StringBuilder logEntry = new StringBuilder();
            foreach (var segment in message.Segments)
            {
                logEntry.Append(segment.Content);
            }
            _logMessages.Add(logEntry.ToString());
        }

        public async Task FlushLogToFileAsync(string filePath)
        {
            using var writer = new StreamWriter(filePath, append: true);
            foreach (var logMessage in _logMessages)
            {
                await writer.WriteLineAsync(logMessage);
            }
        }
    }

}
