using RPGArena.CombatEngine.Core;
using RPGArena.CombatEngine.Decorateur;
using RPGArena.CombatEngine.Enums;
using RPGArena.CombatEngine.Logging;
using RPGArena.CombatEngine.States;
using System;
using System.Linq;
using System.Threading.Tasks;
using ILogger = RPGArena.CombatEngine.Logging.ILogger;

namespace RPGArena.CombatEngine.Skills
{
    public class MangeMort : Skill
    {
        private readonly ILogger _logger;
        public override string Name => "Mangeur de cadavre";
        public override float BaseCooldown { get; set; } = 2;
        public override TypeAttack Type { get; set; } = TypeAttack.Normal;

        public MangeMort(ILogger logger)
        {
            _logger = logger;
        }

        public override async Task Use(Character lanceur, Character target)
        {
            if (!IsReady)
                throw new InvalidOperationException("La compétence n'est pas encore disponible.");

            if (target == null || !target.IsEatable)
                throw new InvalidOperationException("La cible doit être un cadavre pour utiliser cette compétence.");

            if (!target.HasState<IsEaten>())
            {
                target.ApplyOrStackState(new IsEaten(target, _logger));
            }else throw new InvalidOperationException("Le cadavre a déjà été consommé.");

            int gainLife = target.MaxLife / 2;
            lanceur.Life += gainLife;

            _logger.Log($"🧟 {lanceur.Name} dévore {target.Name} et regagne {gainLife} points de vie.");

            await Task.CompletedTask;
        }
    }
}
