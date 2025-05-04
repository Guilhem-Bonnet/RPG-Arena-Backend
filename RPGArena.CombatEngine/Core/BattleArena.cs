using RPGArena.CombatEngine.Characters;
using RPGArena.CombatEngine.Enums;
using RPGArena.CombatEngine.Logging;
using RPGArena.CombatEngine.Services;
using ILogger = RPGArena.CombatEngine.Logging.ILogger;

namespace RPGArena.CombatEngine.Core;

/// <summary>
/// BattleArena est le cœur du moteur de simulation de combat.
/// Elle orchestre les personnages, lance leurs stratégies de manière asynchrone et multithreadée,
/// puis détermine les conditions de victoire ou de défaite.
/// </summary>
public class BattleArena
{
    private readonly List<ICharacter> _characters = new();
    private readonly ILogger _logger;
    private readonly IFightService _fightService;
    private bool _endBattle;

    public IReadOnlyList<ICharacter> Participants => _characters;
    public bool Ended => _endBattle;

    public BattleArena(List<string> names, ILogger logger, IFightService fightService)
    {
        _logger = logger;
        _fightService = fightService;

        var factory = new CharacterFactory(this, logger, fightService);

        foreach (var name in names)
        {
            var character = factory.CreateCharacter("Default", name);
            _characters.Add(character);
        }
    }

    public BattleArena(ILogger logger, IFightService fightService)
    {
        _logger = logger;
        _fightService = fightService;
    }

    public void AddCharacter(ICharacter character)
    {
        if (character == null)
            throw new ArgumentNullException(nameof(character));
        _characters.Add(character);
    }

    public async Task StartBattle()
    {
        _endBattle = false;
        _logger.Log("🟢 Début du combat !");

        // Lancer chaque stratégie sur son propre thread
        var tasks = _characters.Select(c => Task.Run(() => c.ExecuteStrategyAsync())).ToArray();

        // Boucle d’attente jusqu’à ce qu’il n’y ait plus qu’un survivant
        while (_characters.Count(p => p.Life > 0) > 1 &&
               !(_characters.Count(p => p.Life > 0 && p.TypeDuPersonnage != TypePersonnage.MortVivant) == 0))
        {
            await Task.Delay(1000);
        }

        _endBattle = true;

        _logger.Log("🛑 Fin du combat — Résumé des combattants :\n");

        foreach (var c in _characters)
        {
            _logger.Log($"- {c.Name} | Life: {c.Life} | Dead: {c.IsDead} | Eatable: {c.IsEatable} | Attackable: {c.IsAttackable}");
        }

        var survivors = _characters.Where(p => p.Life > 0).ToList();
        if (survivors.Count == 1)
        {
            _logger.Log($"🏆 {survivors[0].Name} est le dernier survivant !");
        }
        else if (survivors.All(s => s.TypeDuPersonnage == TypePersonnage.MortVivant))
        {
            _logger.Log("💀 Les MortVivants ont dominé le champ de bataille !");
        }
        else if (survivors.All(s => s.TypeDuPersonnage == TypePersonnage.Humain))
        {
            _logger.Log("🛡 Les Humains ont dominé le champ de bataille !");
        }
        else
        {
            _logger.Log("☠️ Tous les combattants sont morts. Il n'y a pas de survivants !");
        }

        await Task.WhenAll(tasks); // s’assurer que toutes les stratégies sont terminées
    }
}
