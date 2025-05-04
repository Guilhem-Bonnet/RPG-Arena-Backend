using RPGArena.CombatEngine.Core;
using RPGArena.CombatEngine.Enums;
using RPGArena.CombatEngine.Logging;
using RPGArena.CombatEngine.Services;
using RPGArena.CombatEngine.Skills;
using ILogger = RPGArena.CombatEngine.Logging.ILogger;

namespace RPGArena.CombatEngine.Characters;

public class Guerrier : Character
{
    public Guerrier(string name, BattleArena arena, ILogger logger, IFightService fightService)
        : base(name, arena, logger, fightService)
    {
        TypeDuPersonnage = TypePersonnage.Humain;
        Attack += 2;
        Defense += 2;
    }

    public override async Task PerformActionAsync()
    {
        var target = ChooseTarget();
        var skillAttack = _skills.FirstOrDefault(s => s is AttackBase && s.IsReady);

        if (target != null && skillAttack != null)
        {
            _logger.Log($"🛡️ {Name} attaque courageusement {target.Name}.");
            await skillAttack.Use(this, (Character)target);
        }
    }

    private ICharacter? ChooseTarget()
    {
        var enemies = _arena.Participants
            .Where(p => !p.IsDead && p != this)
            .ToList();

        return enemies.Any() ? enemies[_rand.Next(enemies.Count)] : null;
    }

    public override void AttackBase(Character target)
    {
        var skill = _skills.FirstOrDefault(s => s is AttackBase && s.IsReady);
        if (skill != null)
        {
            _ = skill.Use(this, target);
        }
    }

    public override Task Strategie() => Task.CompletedTask;
}