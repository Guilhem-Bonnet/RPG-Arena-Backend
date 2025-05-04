// RPGArena.CombatEngine/Logging/MultiLogger.cs
using System.Collections.Generic;
/*
/// <summary>
/// MultiLogger permet de diffuser un même message de log à plusieurs destinations (Console, WebSocket, Mongo...).
/// Il implémente ILogger et encapsule plusieurs loggers internes.
/// </summary>
*/

namespace RPGArena.CombatEngine.Logging
{
    public class MultiLogger : ILogger
    {
        private readonly IEnumerable<ILogger> _loggers;

        public MultiLogger(IEnumerable<ILogger> loggers)
        {
            _loggers = loggers;
        }

        public void Log(string message)
        {
            foreach (var logger in _loggers)
            {
                logger.Log(message);
            }
        }
    }
}