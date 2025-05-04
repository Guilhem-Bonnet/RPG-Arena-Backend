using RPGArena.CombatEngine.Core;
using RPGArena.CombatEngine.Enums;
using RPGArena.CombatEngine.Logging;
using RPGArena.CombatEngine.Skills;
using RPGArena.CombatEngine.Services;
using ILogger = RPGArena.CombatEngine.Logging.ILogger;

namespace RPGArena.CombatEngine.Characters;

public class Illusioniste : Character
{
    public Illusioniste(string name, BattleArena arena, ILogger logger, IFightService fightService)
        : base(name, arena, logger, fightService)
    {
    }

    public override async Task PerformActionAsync()
    {
        if (_arena.Participants.Count(p => p is Illusion && !p.IsDead) < 2)
        {
            var clone = new Illusion(this, _arena, _logger, _fightservice);
            _arena.AddCharacter(clone);
            _logger.Log($"✨ {Name} crée une illusion : !");
            return;
        }

        var target = _arena.Participants
            .Where(p => !p.IsDead && p != this)
            .OrderBy(_ => _rand.Next())
            .FirstOrDefault();

        var attack = _skills.FirstOrDefault(s => s is AttackBase && s.IsReady);
        if (target != null && attack != null)
        {
            await attack.Use(this, (Character)target);
        }
    }
    public override Task Strategie() => Task.CompletedTask;
    public override void AttackBase(Character target)
    {
        // L'illusioniste peut faire une attaque normale, ou tu peux la spécialiser.
        var baseAttack = _skills.FirstOrDefault(s => s is ISkill skill && skill.IsReady);
        if (baseAttack != null)
        {
            baseAttack.Use(this, target).Wait();
        }
    }
}


/*
 * 🧠 Suggestions d’amélioration futures :

   Ajouter une compétence propre à l’Illusioniste : CréerIllusion() qui invoque un Illusion dans l’arène.

   Ajout dynamique de clones avec timer d’autodestruction.

   Stratégie auto-défensive ou de diversion avec AI simple.
*/