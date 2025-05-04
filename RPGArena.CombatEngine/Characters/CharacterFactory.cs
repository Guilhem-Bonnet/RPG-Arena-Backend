using System;
using RPGArena.CombatEngine.Core;
using RPGArena.CombatEngine.Enums;
using RPGArena.CombatEngine.Logging;
using RPGArena.CombatEngine.Services;
using ILogger = RPGArena.CombatEngine.Logging.ILogger;

namespace RPGArena.CombatEngine.Characters;

public class CharacterFactory : ICharacterFactory
{
    private readonly BattleArena _arena;
    private readonly ILogger _logger;
    private readonly IFightService _fightService;

    public CharacterFactory(BattleArena arena, ILogger logger, IFightService fightService)
    {
        _arena = arena;
        _logger = logger;
        _fightService = fightService;
    }

    public ICharacter CreateCharacter(string type, string name)
    {
        return type.ToLower() switch
        {
            "alchimiste"   => new Alchimiste(name, _arena, _logger, _fightService),
            "assassin"     => new Assassin(name, _arena, _logger, _fightService),
            "berserker"    => new Berserker(name, _arena, _logger, _fightService),
            "guerrier"     => new Guerrier(name, _arena, _logger, _fightService),
            "illusioniste" => new Illusioniste(name, _arena, _logger, _fightService),
            "magicien"     => new Magicien(name, _arena, _logger, _fightService),
            "necromancien" => new Necromancien(name, _arena, _logger, _fightService),
            "paladin"      => new Paladin(name, _arena, _logger, _fightService),
            "pretre"       => new Pretre(name, _arena, _logger, _fightService),
            "robot"        => new Robot(name, _arena, _logger, _fightService),
            "vampire"      => new Vampire(name, _arena, _logger, _fightService),
            "zombie"       => new Zombie(name, _arena, _logger, _fightService),
            _ => throw new ArgumentException($"❌ Type de personnage inconnu : '{type}'")
        };
    }
}