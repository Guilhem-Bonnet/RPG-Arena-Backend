using System.Linq;
using System.Threading.Tasks;
using RPGArena.CombatEngine.Core;
using RPGArena.CombatEngine.Logging;
using RPGArena.CombatEngine.Services;
using RPGArena.CombatEngine.Skills;
using ILogger = RPGArena.CombatEngine.Logging.ILogger;

namespace RPGArena.CombatEngine.Characters
{
    public class Magicien : Character
    {
        public Magicien(string name, BattleArena arena, ILogger logger, IFightService fightService)
            : base(name, arena, logger, fightService)
        {
            // Initialisation par défaut
            _skills.Add(new AttackBase(fightService, logger));
            Defense = 5;
            Attack = 10;
        }

        public override async Task PerformActionAsync()
        {
            var target = _arena.Participants
                .Where(p => p != this && !p.IsDead && p.IsAttackable)
                .OrderBy(_ => _rand.Next())
                .FirstOrDefault();

            var skill = _skills.FirstOrDefault(s => s.IsReady);

            if (target != null && skill != null)
            {
                await skill.Use(this, (Character)target);
            }
            
        }

        public override void AttackBase(Character target)
        {
            var skill = _skills.FirstOrDefault(s => s.IsReady);
            if (skill != null)
            {
                skill.Use(this, target).Wait();
            }
        }

        public override Task Strategie() => Task.CompletedTask;
    }
}