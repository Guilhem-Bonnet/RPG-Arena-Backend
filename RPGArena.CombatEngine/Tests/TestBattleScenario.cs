using RPGArena.CombatEngine;
using RPGArena.CombatEngine.Characters;
using RPGArena.CombatEngine.Core;
using RPGArena.CombatEngine.Logging;
using RPGArena.CombatEngine.Services;


namespace RPGArena.CombatEngine.Tests
{
    public static class TestBattleScenario
    {
        public static async Task Run()
        {
            var logger = new ConsoleLogger();
            var fightService = new FakeFightService { FixedDamage = 15, ForceCritique = true };

            var arena = new BattleArena(logger);

            var hero = new Pretre("Aelwyn", arena, logger, fightService);
            var enemy = new Zombie("Skraag", arena, logger, fightService);

            arena.AddCharacter(hero);
            arena.AddCharacter(enemy);

            await arena.StartBattle();
        }
    }
}
    

