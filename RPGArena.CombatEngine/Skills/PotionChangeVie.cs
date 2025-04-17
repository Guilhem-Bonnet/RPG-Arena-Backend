using RPGArena.CombatEngine.Core;
using RPGArena.CombatEngine.Enums;
using RPGArena.CombatEngine.Logging;
using System.Threading.Tasks;
using ILogger = RPGArena.CombatEngine.Logging.ILogger;

namespace RPGArena.CombatEngine.Skills
{
    public class PotionChangeLife : Skill
    {
        private readonly ILogger _logger;

        public override string Name => "Potion SwitchLife";
        public override float BaseCooldown { get; set; } = 3.2f;
        public override TypeAttack Type { get; set; } = TypeAttack.Sacre;

        public PotionChangeLife(ILogger logger)
        {
            _logger = logger;
        }

        public override async Task Use(Character lanceur, Character cible)
        {
            if (!cible.IsAttackable)
            {
                _logger.Log($"🚫 La cible {cible.Name} n'est pas attaquable !");
                return;
            }

            int lifeLanceur = lanceur.Life;
            int lifeCible = Math.Min(cible.Life, lanceur.MaxLife);

            lanceur.Life = lifeCible;
            cible.Life = lifeLanceur;

            _logger.Log($"🔁 {lanceur.Name} échange sa vie avec {cible.Name} !");
            _logger.Log($"💚 {cible.Name} : {cible.Life} PV | {lanceur.Name} : {lanceur.Life} PV");

            await Task.CompletedTask;
        }
    }
}
