using RPGArena.CombatEngine.Core;
using RPGArena.CombatEngine.Enums;
using RPGArena.CombatEngine.Logging;
using RPGArena.CombatEngine.Skills;
using RPGArena.CombatEngine.Services;
using ILogger = RPGArena.CombatEngine.Logging.ILogger;

namespace RPGArena.CombatEngine.Characters;

public class Alchimiste : Character
{
    public Alchimiste(string name, BattleArena arena, ILogger logger, IFightService fightService)
        : base(name, arena, logger, fightService)
    {
        Defense = 2;
        _skills[0] = new JetDePotion(_fightservice, _logger);
        _skills.Add(new PotionChangeLife(_logger));
    }

    public override async Task ExecuteStrategy()
    {
        var target = ChooseTarget();
        var swapTarget = ChooseBestAllyForSwitch();

        var skillSwitch = _skills.FirstOrDefault(s => s is PotionChangeLife && s.IsReady);
        var skillAttack = _skills.FirstOrDefault(s => s is JetDePotion && s.IsReady);

        if (skillSwitch != null && swapTarget != null && Life <= MaxLife * 0.8)
        {
            await skillSwitch.Use(this, swapTarget);
            return;
        }

        if (skillAttack != null && target != null)
        {
            await skillAttack.Use(this, target);
        }
    }

    public override void AttackBase(Character target) { }

    public override Task Strategie() => Task.CompletedTask;

    private Character ChooseTarget()
    {
        var mortVivants = _arena.Participants
            .Where(p => p.Life > 0 && p.TypeDuPersonnage == TypePersonnage.MortVivant && !p.IsDead)
            .OfType<Character>() // filtre + cast
            .ToList();

        if (mortVivants.Any())
            return mortVivants[_rand.Next(mortVivants.Count)];

        var others = _arena.Participants
            .Where(p => p != this && !p.IsDead)
            .OfType<Character>()
            .ToList();

        return others.Count > 0 ? others[_rand.Next(others.Count)] : null;
    }

    private Character ChooseBestAllyForSwitch()
    {
        var potential = _arena.Participants
            .Where(p => !p.IsDead && p != this)
            .OrderByDescending(p => p.Life)
            .OfType<Character>()
            .FirstOrDefault();

        return (potential?.Life > Life) ? potential : null;
    }

}
