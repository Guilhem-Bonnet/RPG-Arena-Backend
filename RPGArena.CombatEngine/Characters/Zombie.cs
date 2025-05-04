using RPGArena.CombatEngine.Core;
using RPGArena.CombatEngine.Enums;
using RPGArena.CombatEngine.Logging;
using RPGArena.CombatEngine.Skills;
using RPGArena.CombatEngine.Services;
using ILogger = RPGArena.CombatEngine.Logging.ILogger;

namespace RPGArena.CombatEngine.Characters;

public class Zombie : Character
{
    private const int SEUIL_SANTE = 70;

    public Zombie(string name, BattleArena arena, ILogger logger, IFightService fightService)
        : base(name, arena, logger, fightService)
    {
        TypeDuPersonnage = TypePersonnage.MortVivant;
        Defense = 0;
        _skills.Add(new MangeMort(_logger));
    }

    public override void AttackBase(Character target)
    {
        BasicAttack(target);
    }

    public override async Task PerformActionAsync()
    {
        var skillMangeMort = _skills.FirstOrDefault(c => c is MangeMort && c.IsReady);
        var skillAttack = _skills.FirstOrDefault(c => c is AttackBase && c.IsReady);

        // Si la vie est basse, tenter de manger un cadavre
        if (Life <= SEUIL_SANTE && skillMangeMort is MangeMort mangeMort)
        {
            var cadavre = _arena.Participants.FirstOrDefault(p => p.IsEatable);
            if (cadavre != null)
            {
                _logger.Log($"🧟 {Name} utilise {mangeMort.Name} sur {cadavre.Name} !");
                await mangeMort.Use(this, (Character)cadavre);
                return;
            }
        }

        // Sinon attaque un non-MortVivant
        var vivants = _arena.Participants
            .Where(p => !p.IsDead && p.TypeDuPersonnage != TypePersonnage.MortVivant && p != this)
            .ToList();

        if (vivants.Any() && skillAttack is not null)
        {
            var cible = vivants[_rand.Next(vivants.Count)];
            _logger.Log($"🧟 {Name} attaque {cible.Name} !");
            await skillAttack.Use(this, cible);
        }
    }

    public override Task Strategie()
    {
        throw new NotImplementedException();
    }
}
