// RPGArena.CombatEngine/Logging/MultiLogger.cs
using System.Collections.Generic;

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