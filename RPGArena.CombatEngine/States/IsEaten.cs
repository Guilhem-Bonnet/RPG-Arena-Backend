using RPGArena.CombatEngine.Core;
using ILogger = RPGArena.CombatEngine.Logging.ILogger;

namespace RPGArena.CombatEngine.States
{
    public class IsEaten : State
    {
        public override string Name => "Dévoré";

        public IsEaten(Character target, ILogger logger) : base(target, logger)
        {
            Duration = int.MaxValue; // jamais retiré automatiquement
        }

        public override async Task OnStart()
        {
            _logger?.Log($"☠️ {Target.Name} a été dévoré et ne peut plus être ciblé.");
            await Task.CompletedTask;
        }
    }

}
