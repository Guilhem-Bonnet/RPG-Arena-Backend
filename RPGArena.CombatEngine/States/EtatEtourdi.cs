using RPGArena.CombatEngine.Characters;
using RPGArena.CombatEngine.Core;
using System;
using System.Threading;
using System.Threading.Tasks;
using RPGArena.CombatEngine.States;
using ILogger = RPGArena.CombatEngine.Logging.ILogger;

namespace RPGArena.CombatEngine.States
{
    /// <summary>
    /// Effet d'étourdissement : empêche le personnage de lancer des attaques pendant une durée temporaire.
    /// </summary>
    public class EtatEtourdi : State
    {
        private readonly CancellationTokenSource _cts = new();
        private readonly int _durationMs;
        private readonly ILogger _logger;

        public override string Name => "Étourdissant";

        public EtatEtourdi(Character target, ILogger logger, int durationMs = 5000) : base(target)
        {
            _durationMs = durationMs;
            _logger = logger;
        }

        public override async Task OnStart()
        {
            Target.CanAct = false;
            _logger.Log($"💫 {Target.Name} est étourdi pour {_durationMs / 1000} secondes !");
            try
            {
                await Task.Delay(_durationMs, _cts.Token);
            }
            catch (TaskCanceledException)
            {
                return;
            }
            Target.CanAct = true;
            Target.RemoveState<EtatEtourdi>();
            _logger.Log($"✅ {Target.Name} n'est plus étourdi.");
        }

        public override void End()
        {
            _cts.Cancel();
            Target.CanAct = true;
            base.End();
        }
    }
}