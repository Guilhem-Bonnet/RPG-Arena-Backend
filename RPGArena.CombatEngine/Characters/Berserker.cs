using RPGArena.CombatEngine.Core;
using RPGArena.CombatEngine.Enums;
using RPGArena.CombatEngine.Logging;
using RPGArena.CombatEngine.Services;
using RPGArena.CombatEngine.Skills;
using ILogger = RPGArena.CombatEngine.Logging.ILogger;

namespace RPGArena.CombatEngine.Characters;

public class Berserker : Character
{
    public Berserker(string name, BattleArena arena, ILogger logger, IFightService fightService)
        : base(name, arena, logger, fightService)
    {
        TypeDuPersonnage = TypePersonnage.Humain;
        Attack += 5; // boost offensif
        Defense -= 2; // fragile
    }

    public override async Task PerformActionAsync()
    {
        var enemies = _arena.Participants
            .Where(p => !p.IsDead && p != this)
            .ToList();

        if (!enemies.Any()) return;

        var target = enemies[_rand.Next(enemies.Count)];
        var attackSkill = _skills.FirstOrDefault(s => s is AttackBase && s.IsReady);

        if (attackSkill != null)
        {
            _logger.Log($"🪓 {Name} fonce sur {target.Name} avec rage !");
            await attackSkill.Use(this, (Character)target);
        }
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