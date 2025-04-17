// RPGArena.CombatEngine/States/EtatAcide.cs
using RPGArena.CombatEngine.Core;
using RPGArena.CombatEngine.Logging;
using System.Threading.Tasks;
using ILogger = RPGArena.CombatEngine.Logging.ILogger;


/*
Effet : Acide
- Retire de la Life à chaque fois que l'effet est déclenché sur la target 
- Réduit sa défense pendant 5 secondes (cumulable)

Exemple d'utilisation :

Attack de l'acide : 2 points de Life retirés et -5 points de défense pendant 5 secondes

---Attente de 1 secondes---

Acide : -5 points de défense pendant 4 secondes

---Attente de 1 secondes---

Acide : -5 points de défense pendant 3 secondes
Attack de l'acide : 2 points de Life retirés et -10 points de défense pendant 5 secondes
...

*/

namespace RPGArena.CombatEngine.States
{
    public class Acide : State
    {
        public override string Name => "Acide";
        private readonly ILogger _logger;

        public Acide(Character target, ILogger logger) : base(target, logger)
        {
            _logger = logger;
            Duration = 5;
        }

        public override async Task OnStart()
        {
            if (_cts.IsCancellationRequested)
                return;

            _logger.Log($"🧪 {Target.Name} est attaqué par de l'acide !");
            Target.Life -= 2;

            for (int i = 0; i < Duration; i++)
            {
                if (_cts.Token.IsCancellationRequested)
                    return;

                int defenseLoss = 5 * StackCount;
                Target.Defense -= defenseLoss;

                _logger.Log($"🛡 {Target.Name} perd {defenseLoss} points de défense (Acide – {Duration - i}s restantes)");
                await Task.Delay(1000, _cts.Token);
            }

            ResetStack();
            _logger.Log($"✅ L'effet d'acide sur {Target.Name} s'est dissipé.");
        }

        public override void Stack()
        {
            base.Stack();
            _logger.Log($"➕ L'acide se renforce sur {Target.Name} (Cumul = {StackCount})");
            _ = OnStart(); // Redéclenche l’effet avec cumul mis à jour
        }

        public override void End()
        {
            _logger.Log($"❌ Fin de l'effet acide sur {Target.Name}");
            base.End();
        }
    }
}

