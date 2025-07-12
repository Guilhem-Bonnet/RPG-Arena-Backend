// RPGArena.CombatEngine/Skills/Soin.cs
using RPGArena.CombatEngine.Core;
using RPGArena.CombatEngine.Enums;
using RPGArena.CombatEngine.Logging;
using RPGArena.CombatEngine.Services;
using RPGArena.CombatEngine.States;
using ILogger = RPGArena.CombatEngine.Logging.ILogger;

namespace RPGArena.CombatEngine.Skills;

public class Soin : Skill
{
    private readonly IFightService _fightService;
    private readonly ILogger _logger;

    public override string Name => "Soin";
    public override float BaseCooldown { get; set; } = 2.5f;
    public override TypeAttack Type { get; set; } = TypeAttack.Sacre;

    public int ValueSoin { get; set; } = 15;
    public override int ValueDommage { get; set; } = 20;

    public Soin(IFightService fightService, ILogger logger)
    {
        _fightService = fightService;
        _logger = logger;
    }

    public override async Task Use(Character lanceur, Character cible)
    {
        if (!IsReady) return;

        _logger.Log($"✨ {lanceur.Name} utilise {Name} sur {cible.Name}.");

        if (cible.TypeDuPersonnage == TypePersonnage.MortVivant)
        {
            var resultAttack = lanceur.LancerDe();
            var resultDefense = cible.LancerDe();
            
            int damage = _fightService.CalculateDamage(lanceur, cible, Type, resultAttack, resultDefense,this);
            _fightService.InflictDamage(cible, damage);
            
        }
        else 
        {
            if (cible.HasState<Empoisonne>())
            {
                cible.RemoveState<Empoisonne>();
                _logger.Log($"💚 {cible.Name} est soigné du poison !");
            }

            cible.Life += ValueSoin;
            _logger.Log($"💊 {cible.Name} regagne {ValueSoin} points de vie grâce à {Name}.");
        }
        Cooldown = BaseCooldown;
        
        await Task.CompletedTask;
    }
}
