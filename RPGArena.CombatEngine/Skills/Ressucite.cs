// RPGArena.CombatEngine/Skills/Ressucite.cs
using System.Threading.Tasks;
using RPGArena.CombatEngine.Core;
using RPGArena.CombatEngine.Enums;
using RPGArena.CombatEngine.Logging;
using ILogger = RPGArena.CombatEngine.Logging.ILogger;

namespace RPGArena.CombatEngine.Skills;

public class Ressucite : Skill
{
    private readonly ILogger _logger;

    public Ressucite(ILogger logger)
    {
        _logger = logger;
    }

    public override string Name => "Mangeur de cadavre";
    public override float BaseCooldown { get; set; } = 10;
    public override TypeAttack Type { get; set; } = TypeAttack.Sacre;

    public override async Task Use(Character lanceur, Character cible)
    {
        if (cible.IsDead)
        {
            _logger.Log($"🧟 {lanceur.Name} ressuscite {cible.Name} !");

            // Restaure l'état original et les PV
            cible = cible.etatOriginal; // WIP
            cible.Life = 100;

            _logger.Log($"✨ {cible.Name} revient à la vie avec {cible.Life} points de vie !");

            ReduceRecharge();
        }
        else
        {
            _logger.Log($"❌ {cible.Name} est encore en vie, la résurrection est impossible.");
        }

        await Task.CompletedTask;
    }
}
