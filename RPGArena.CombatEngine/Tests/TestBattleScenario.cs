using RPGArena.CombatEngine.Characters;
using RPGArena.CombatEngine.Core;
using RPGArena.CombatEngine.Logging;
using RPGArena.CombatEngine.Services;

namespace RPGArena.CombatEngine.Tests
{
    /// <summary>
    /// Ce scénario de test initialise une arène de combat avec un prêtre et un zombie
    /// en utilisant un service de combat simulé avec des résultats forcés.
    /// </summary>
    public static class TestBattleScenario
    {
        public static async Task Run()
        {
            var logger = new ConsoleLogger();
            var fightService = new FightService(logger);


            var arena = new BattleArena(logger, fightService);

            var pretre = new Pretre("Aelwyn", arena, logger, fightService);
            var pretre2 = new Pretre("Sparadra", arena, logger, fightService);
            
            var Vampire = new Vampire("Dracula", arena, logger, fightService);
            var Vampire2 = new Vampire("Drakul", arena, logger, fightService);
            var Vampire3 = new Vampire("Nosferatu", arena, logger, fightService);
            var Vampire4 = new Vampire("Alucard", arena, logger, fightService);
            var Vampire5 = new Vampire("Carmilla", arena, logger, fightService);
            var Vampire6 = new Vampire("Lilith", arena, logger, fightService);
            
            var zombie1 = new Zombie("Skraag", arena, logger, fightService);
            var zombie2 = new Zombie("Mog", arena, logger, fightService);
            var zombie3 = new Zombie("Gag", arena, logger, fightService);
            var zombie4 = new Zombie("Bic", arena, logger, fightService);
            var zombie5 = new Zombie("Horg", arena, logger, fightService);
            
            var Guerrier = new Guerrier("Brutus", arena, logger, fightService);
            var Magicien = new Magicien("Merlin", arena, logger, fightService);
            var Assassin = new Assassin("Shadow", arena, logger, fightService);
            var Paladin = new Paladin("Lancelot", arena, logger, fightService);
            
            var Berserker = new Berserker("Ragnar", arena, logger, fightService);
            var Alchimiste = new Alchimiste("Elixir", arena, logger, fightService);
            var Illusionniste = new Illusioniste("Myst", arena, logger, fightService);
            var Robot = new Robot("Cyborg", arena, logger, fightService);
            var Necromancien = new Necromancien("Nécro", arena, logger, fightService);

            arena.AddCharacter(pretre);
            arena.AddCharacter(Paladin);
            //arena.AddCharacter(Illusionniste);
            arena.AddCharacter(Robot);
            
            arena.AddCharacter(Guerrier);
            arena.AddCharacter(Magicien);
            arena.AddCharacter(Assassin);
            
            arena.AddCharacter(Berserker);
            arena.AddCharacter(Alchimiste);
            


            arena.AddCharacter(Vampire);
            /*
            arena.AddCharacter(Vampire2);
            arena.AddCharacter(Vampire3);
            arena.AddCharacter(Vampire4);
            arena.AddCharacter(Vampire5);
            arena.AddCharacter(Vampire6);
            
            arena.AddCharacter(zombie1);
            arena.AddCharacter(zombie2);
            
            arena.AddCharacter(zombie3);
            arena.AddCharacter(zombie4);
            arena.AddCharacter(zombie5);
            */
            arena.AddCharacter(Necromancien);
            


            await arena.StartBattle();
        }
    }
}