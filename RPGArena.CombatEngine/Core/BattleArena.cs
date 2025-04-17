using RPGArena.CombatEngine.Characters;
using RPGArena.CombatEngine.Enums;
using ILogger = RPGArena.CombatEngine.Logging.ILogger;

namespace RPGArena.CombatEngine.Core;

public class BattleArena
{
    private readonly List<ICharacter> _characters = new();
    private readonly ILogger _logger;

    private bool EndBattle;

    public IReadOnlyList<ICharacter> Participants => _characters;

    public bool Ended => EndBattle;

    public BattleArena(List<string> names, ILogger logger)
    {
        _logger = logger;
        var factory = new CharacterFactory();

        foreach (var name in names)
        {
            var character = factory.CreateCharacter("Default", name);
            _characters.Add(character);
        }
    }

    public BattleArena(ILogger logger)
    {
        _logger = logger;
    }

    public void AddCharacter(ICharacter character)
    {
        if (character == null)
            throw new ArgumentNullException(nameof(character));
        _characters.Add(character);
    }

    public async Task StartBattle()
    {
        EndBattle = false;
        _logger.Log("🟢 Début du combat !");

        // Replace the following line:
        var tasks = _characters.Select(participant => Task.Run(() => participant.ExecuteStrategy())).ToArray();

        // With this corrected line:
        var tasks2 = _characters.Select(participant => Task.Run(() => participant.Strategie())).ToArray();

        while (_characters.Count(p => p.Life > 0) > 1 &&
               !(_characters.Count(p => p.Life > 0 && p.TypeDuPersonnage != TypePersonnage.MortVivant) == 0))
        {
            await Task.Delay(1000);
        }

        EndBattle = true;

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
    }
}
