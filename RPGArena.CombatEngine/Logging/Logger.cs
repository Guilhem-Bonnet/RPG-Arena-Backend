using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace RPGArena.CombatEngine.Logging
{
    /// <summary>
    /// Logger singleton pour stocker les messages de combat et les écrire dans un fichier.
    /// Utile pour déboguer ou historiser un combat.
    /// </summary>
    public class Logger
    {
        private static readonly Lazy<Logger> _instance = new Lazy<Logger>(() => new Logger());
        private readonly List<string> _logMessages = new List<string>();

        public static Logger Instance => _instance.Value;

        /// <summary>
        /// Ajoute un message de combat au log.
        /// </summary>
        public void Log(Message message)
        {
            if (message?.Segments == null)
                return;

            StringBuilder logEntry = new StringBuilder();

            foreach (var segment in message.Segments)
            {
                logEntry.Append(segment?.Content ?? string.Empty);
            }

            _logMessages.Add(logEntry.ToString());
        }

        /// <summary>
        /// Écrit tous les logs accumulés dans un fichier de sortie.
        /// </summary>
        public async Task FlushLogToFileAsync(string filePath)
        {
            using var writer = new StreamWriter(filePath, append: true);
            foreach (var logMessage in _logMessages)
            {
                await writer.WriteLineAsync(logMessage);
            }
        }

        /// <summary>
        /// Vide les logs en mémoire (optionnel).
        /// </summary>
        public void Clear()
        {
            _logMessages.Clear();
        }
    }
}