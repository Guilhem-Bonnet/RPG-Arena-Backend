using RPGArena.CombatEngine.Core;
using RPGArena.CombatEngine.Enums;
using RPGArena.CombatEngine.Logging;
using RPGArena.CombatEngine.Services;
using RPGArena.CombatEngine.Skills;
using ILogger = RPGArena.CombatEngine.Logging.ILogger;

namespace RPGArena.CombatEngine.Characters;

public class Assassin : Character
{
    public Assassin(string name, BattleArena arena, ILogger logger, IFightService fightService)
        : base(name, arena, logger, fightService)
    {
        TypeDuPersonnage = TypePersonnage.Humain;
    }

    public override async Task PerformActionAsync()
    {
        var target = _arena.Participants
            .Where(p => !p.IsDead && p != this)
            .OrderBy(p => p.Life)
            .FirstOrDefault();

        var skill = _skills.FirstOrDefault(s => s.IsReady);

        if (target != null && skill != null)
        {
            await skill.Use(this, (Character)target);
        }
    }

    public override void AttackBase(Character target)
    {
        var baseAttack = _skills.FirstOrDefault(s => s is AttackBase && s.IsReady);
        if (baseAttack != null)
        {
            _ = baseAttack.Use(this, target); // Async fire & forget
        }
    }

    public override Task Strategie() => Task.CompletedTask;
}