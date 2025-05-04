// RPGArena.CombatEngine/ISkills/Ressucite.cs
using RPGArena.CombatEngine.Core;
using RPGArena.CombatEngine.Enums;
using RPGArena.CombatEngine.Logging;
using RPGArena.CombatEngine.Skills;
using ILogger = RPGArena.CombatEngine.Logging.ILogger;
using RPGArena.CombatEngine.Characters;
using RPGArena.CombatEngine.States;

namespace RPGArena.CombatEngine.ISkills;

public class Ressucite : Skill
{
    public override string Name => "Ressusciter";
    public override float BaseCooldown { get; set; } = 10f;
    public override TypeAttack Type { get; set; } = TypeAttack.Sacre;

    private readonly ILogger _logger;

    public Ressucite(ILogger logger)
    {
        _logger = logger;
    }

    public override async Task Use(Character lanceur, Character cible)
    {
        if (!IsReady)
        {
            _logger.Log($"❌ {Name} n'est pas prête.");
            return;
        }

        if (!cible.IsDead)
        {
            _logger.Log($"⚠️ {cible.Name} est encore en vie. Impossible de ressusciter un être vivant.");
            return;
        }

        if (cible is Illusion)
        {
            _logger.Log($"🚫 {Name} ne peut pas être utilisé sur une illusion.");
            return;
        }

        if (cible.OriginalCharacter == null)
        {
            _logger.Log($"❌ {cible.Name} ne possède pas d'état original à restaurer.");
            return;
        }

        var resurrected = cible.OriginalCharacter;

        resurrected.Life = resurrected.MaxLife / 2;
        resurrected.RemoveState<IsEaten>();
        lanceur.Arena.AddCharacter(resurrected);

        _logger.Log($"✨ {lanceur.Name} ressuscite {resurrected.Name} avec {resurrected.Life} points de vie !");
        Cooldown = BaseCooldown;
    }
}