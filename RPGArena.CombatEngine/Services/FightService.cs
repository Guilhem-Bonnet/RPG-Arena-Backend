// RPGArena.CombatEngine/Services/FightService.cs
using RPGArena.CombatEngine.Core;
using RPGArena.CombatEngine.Enums;
using ILogger = RPGArena.CombatEngine.Logging.ILogger;

namespace RPGArena.CombatEngine.Services;

public class FightService : IFightService
{
    private readonly ILogger _logger;

    public FightService(ILogger logger)
    {
        _logger = logger;
    }

    public int CalculateDamage(Character attacker, Character defender, TypeAttack typeAttack, ResultDe resultAttack, ResultDe resultDefense)
    {
        LogDiceOutcome(attacker, resultAttack, true);
        LogDiceOutcome(defender, resultDefense, false);

        if (resultAttack == ResultDe.RéussiteCritique && resultDefense == ResultDe.RéussiteCritique)
        {
            _logger.Log($"✨ Choc épique entre {attacker.Name} et {defender.Name} : une maîtrise incroyable !");
        }

        if (resultAttack == ResultDe.EchecCritique && resultDefense == ResultDe.RéussiteCritique)
        {
            _logger.Log($"⚔ {attacker.Name} tente une attaque risquée, mais {defender.Name} contre avec brio !");
            int counterDamage = (int)(defender.Attack * 2);
            _logger.Log($"💥 Contre-attaque dévastatrice : {defender.Name} inflige {counterDamage} dégâts à {attacker.Name}.");
            attacker.Life -= counterDamage;
            return 0;
        }

        int baseAttack = ApplyAttackMultiplier(resultAttack, attacker.Attack);
        int baseDefense = ApplyDefenseMultiplier(resultDefense, defender.Defense);

        int damage = Math.Max(baseAttack - baseDefense, 0);
        damage = ApplyTypeEffects(damage, typeAttack, attacker, defender);

        _logger.Log($"🗡 {attacker.Name} inflige {damage} dégâts à {defender.Name}.");
        defender.Life -= damage;

        return damage;
    }

    private void LogDiceOutcome(Character character, ResultDe result, bool isAttacker)
    {
        string role = isAttacker ? "Attaquant" : "Défenseur";
        string reaction = result switch
        {
            ResultDe.EchecCritique => $"❌ {role} {character.Name} rate lamentablement son action !",
            ResultDe.Echec => $"⚠️ {role} {character.Name} est déstabilisé, rien ne se passe.",
            ResultDe.Neutre => $"{role} {character.Name} garde son sang-froid.",
            ResultDe.Réussite => $"✅ {role} {character.Name} agit efficacement.",
            ResultDe.RéussiteCritique => $"🌟 {role} {character.Name} effectue une action exceptionnelle !",
            _ => string.Empty
        };

        if (!string.IsNullOrEmpty(reaction))
        {
            _logger.Log(reaction);
        }
    }

    private int ApplyAttackMultiplier(ResultDe result, int attack)
    {
        return result switch
        {
            ResultDe.EchecCritique => 0,
            ResultDe.Echec => 0,
            ResultDe.Neutre => attack,
            ResultDe.Réussite => (int)(attack * 1.5),
            ResultDe.RéussiteCritique => attack * 2,
            _ => attack
        };
    }

    private int ApplyDefenseMultiplier(ResultDe result, int defense)
    {
        return result switch
        {
            ResultDe.EchecCritique => 0,
            ResultDe.Echec => 0,
            ResultDe.Neutre => defense,
            ResultDe.Réussite => (int)(defense * 1.5),
            ResultDe.RéussiteCritique => defense * 2,
            _ => defense
        };
    }

    private int ApplyTypeEffects(int damage, TypeAttack type, Character attacker, Character defender)
    {
        var multiplier = GetEffectMultiplier(type, defender.TypeDuPersonnage);

        if (multiplier == 0.0)
        {
            _logger.Log($"🛡 Immunité : {defender.Name} est totalement immunisé contre {type}.");
            return 0;
        }
        else if (multiplier > 1.0)
        {
            int bonus = (int)(damage * (multiplier - 1));
            damage += bonus;
            _logger.Log($"⚡ Vulnérabilité : dégâts augmentés x{multiplier:F1} => +{bonus} dégâts.");
        }
        else if (multiplier < 1.0)
        {
            int reduced = (int)(damage * (1 - multiplier));
            damage = (int)(damage * multiplier);
            _logger.Log($"🛡 Résistance : dégâts réduits x{multiplier:F1} => -{reduced} dégâts.");
        }

        return damage;
    }

    private double GetEffectMultiplier(TypeAttack attackType, TypePersonnage targetType)
    {
        return (attackType, targetType) switch
        {
            (TypeAttack.Sacre, TypePersonnage.MortVivant) => 1.5,
            (TypeAttack.Tenebre, TypePersonnage.Humain) => 1.2,
            (TypeAttack.Feu, TypePersonnage.Glace) => 2.0,
            (TypeAttack.Glace, TypePersonnage.Feu) => 0.5,
            (TypeAttack.Normal, TypePersonnage.Robot) => 0.8,
            (TypeAttack.Glace, TypePersonnage.Glace) => 0.0, // immunité
            _ => 1.0
        };
    }
}
