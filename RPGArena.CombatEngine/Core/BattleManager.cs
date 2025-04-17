using ILogger = RPGArena.CombatEngine.Logging.ILogger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGArena.CombatEngine.Core
{
    internal class BattleManager
    {
        public BattleManager(ICharacterFactory characterFactory, ICombatRepository repository, ILogger logger)
        {
            _characterFactory = characterFactory;
            _repository = repository;
            _logger = logger; // MultiLogger
        }

    }
}
