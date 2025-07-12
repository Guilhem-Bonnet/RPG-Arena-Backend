// RPGArena.CombatEngine/ISkills/AttackBase.cs
using RPGArena.CombatEngine.Core;
using RPGArena.CombatEngine.Enums;
using RPGArena.CombatEngine.Services;
using ILogger = RPGArena.CombatEngine.Logging.ILogger;

namespace RPGArena.CombatEngine.Skills;

public class AttackBase : Skill
{
    private readonly IFightService _fightService;
    protected readonly ILogger _logger;

    public AttackBase(IFightService fightService, ILogger logger)
    {
        _fightService = fightService;
        _logger = logger;
        Type = TypeAttack.Normal;
        BaseCooldown = 1;
        Cooldown = 0;
    }

    public override string Name => "Attaque de base";
    public override async Task Use(Character lanceur, Character cible)
    {
        if (!cible.IsAttackable)
        {
            _logger.Log($"🚫 La cible {cible.Name} n'est pas attaquable !");
            return;
        }

        _logger.Log($"🔷 {lanceur.Name} utilise {Name} sur {cible.Name}.");

        var resultAttack = lanceur.LancerDe();
        var resultDefense = cible.LancerDe();

        int damage = _fightService.CalculateDamage(lanceur, cible, Type, resultAttack, resultDefense);
        _fightService.InflictDamage(cible, damage);
        await Task.CompletedTask;
    }
}