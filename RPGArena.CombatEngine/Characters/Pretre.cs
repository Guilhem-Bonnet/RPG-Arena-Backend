using RPGArena.CombatEngine.Core;
using RPGArena.CombatEngine.Enums;
using RPGArena.CombatEngine.Skills;
using RPGArena.CombatEngine.Services;
using RPGArena.CombatEngine.Logging;
using ILogger = RPGArena.CombatEngine.Logging.ILogger;

namespace RPGArena.CombatEngine.Characters;

public class Pretre : Character
{
    public Pretre(string name, BattleArena arena, ILogger logger, IFightService fightService)
        : base(name, arena, logger, fightService)
    {
        _skills[0].Type = TypeAttack.Sacre;
        _skills.Add(new Soin(_fightservice, _logger));
    }

    public override void AttackBase(Character target)
    {
        throw new NotImplementedException();
    }

    public override async Task ExecuteStrategy()
    {
        var target = ChooseTarget();

        var skillSoin = _skills.FirstOrDefault(s => s is Soin && s.IsReady);
        var skillAttack = _skills.FirstOrDefault(s => s is AttackBase && s.IsReady);

        // 🔧 Priorité : se soigner soi-même si PV bas
        if (skillSoin != null && Life <= MaxLife * 0.5)
        {
            await skillSoin.Use(this, this);
            return;
        }

        // 🧟 Si mort-vivant ciblable, tenter un soin offensif
        if (skillSoin != null && target.TypeDuPersonnage == TypePersonnage.MortVivant && target.Life <= 100)
        {
            await skillSoin.Use(this, target);
            return;
        }

        // 🗡 Sinon, attaque de base
        if (skillAttack != null)
        {
            await skillAttack.Use(this, target);
        }
    }

    public override Task Strategie()
    {
        throw new NotImplementedException();
    }

    private Character ChooseTarget()
    {
        var possibleTargets = _arena.Participants
            .Where(p => p != this && !p.IsDead)
            .ToList();

        var mortsVivants = possibleTargets
            .Where(p => p.TypeDuPersonnage == TypePersonnage.MortVivant)
            .ToList();

        return (Character)(mortsVivants.Any()
            ? mortsVivants[_rand.Next(mortsVivants.Count)]
            : possibleTargets[_rand.Next(possibleTargets.Count)]);
    }
}
