// RPGArena.CombatEngine/Skills/JetDePotion.cs
using RPGArena.CombatEngine.Core;
using RPGArena.CombatEngine.Enums;
using RPGArena.CombatEngine.Logging;
using RPGArena.CombatEngine.Services;
using RPGArena.CombatEngine.States;
using ILogger = RPGArena.CombatEngine.Logging.ILogger;

namespace RPGArena.CombatEngine.Skills;

public class JetDePotion : AttackBase
{
    public JetDePotion(IFightService fightService, ILogger logger)
        : base(fightService, logger)
    {
        Type = TypeAttack.Normal;
    }

    public override string Name => "Jet de fiole";

    public override async Task Use(Character lanceur, Character cible)
    {
        if (cible != null && !cible.IsDead)
        {
            cible.ApplyOrStackState(new Empoisonne(cible, _logger));
        }

        await base.Use(lanceur, cible);
    }
}
