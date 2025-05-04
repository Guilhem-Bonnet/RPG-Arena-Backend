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
            var fightService = new FakeFightService
            {
                FixedDamage = 15,
                ForceCritique = true
            };

            var arena = new BattleArena(logger, fightService);

            var pretre = new Pretre("Aelwyn", arena, logger, fightService);
            var zombie = new Zombie("Skraag", arena, logger, fightService);

            arena.AddCharacter(pretre);
            arena.AddCharacter(zombie);

            await arena.StartBattle();
        }
    }
}