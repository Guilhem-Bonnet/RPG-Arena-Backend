using System.Linq;
using System.Threading.Tasks;
using RPGArena.CombatEngine.Core;
using RPGArena.CombatEngine.Enums;
using RPGArena.CombatEngine.Logging;
using RPGArena.CombatEngine.Skills;
using RPGArena.CombatEngine.Services;
using ILogger = RPGArena.CombatEngine.Logging.ILogger;

namespace RPGArena.CombatEngine.Characters;

public class Vampire : Character
{
    public Vampire(string name, BattleArena arena, ILogger logger, IFightService fightService)
        : base(name, arena, logger, fightService)
    {
        TypeDuPersonnage = TypePersonnage.MortVivant;

        // TODO: remplacer par une vraie compétence de vampirisme
        _skills[0].Type = TypeAttack.Normal; // attaque de base par défaut
    }

    public override void AttackBase(Character target)
    {
        // Le Vampire peut avoir une attaque personnalisée plus tard
        throw new NotImplementedException("AttackBase should be implemented with a vampiric attack.");
    }

    public override async Task PerformActionAsync()
    {
        var target = ChooseTarget();

        var attackSkill = _skills.FirstOrDefault(s => s is AttackBase && s.IsReady);
        if (attackSkill != null && target != null)
        {
            await attackSkill.Use(this, target);
        }
    }

    public override Task Strategie()
    {
        throw new NotImplementedException();
    }

    private Character ChooseTarget()
    {
        var validTargets = _arena.Participants
            .Where(p => !p.IsDead && p != this && p.IsAttackable)
            .ToList();

        if (validTargets.Count == 0)
            return null;

        return (Character)validTargets[_rand.Next(validTargets.Count)];
    }
}