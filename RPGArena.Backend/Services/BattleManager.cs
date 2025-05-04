using RPGArena.CombatEngine.Characters;
using RPGArena.CombatEngine.Core;
using RPGArena.CombatEngine.Logging;
using RPGArena.CombatEngine.Services;
using RPGArena.Backend.Repositories;
using ILogger = RPGArena.CombatEngine.Logging.ILogger;

namespace RPGArena.Backend.Services;

/// <summary>
/// Gère la création d'une arène, l'ajout des personnages, l'exécution du combat,
/// puis l'enregistrement du résultat final en base.
/// </summary>
public class BattleManager
{
    private readonly ICharacterFactory _characterFactory;
    private readonly ICombatRepository _repository;

    public BattleManager(ICharacterFactory characterFactory, ICombatRepository repository)
    {
        _characterFactory = characterFactory;
        _repository = repository;
    }

    /// <summary>
    /// Lance un combat avec les personnages fournis et enregistre le résultat.
    /// </summary>
    public async Task RunBattleAsync(List<(string Type, string Name)> characterSpecs, ILogger logger)
    {
        var fightService = new FightService(logger);
        var arena = new BattleArena(logger, fightService);

        foreach (var (type, name) in characterSpecs)
        {
            var character = _characterFactory.CreateCharacter(type, name);
            arena.AddCharacter(character);
        }

        await arena.StartBattle();

        await _repository.SaveCombatResult(arena.Participants.ToList());
    }
}