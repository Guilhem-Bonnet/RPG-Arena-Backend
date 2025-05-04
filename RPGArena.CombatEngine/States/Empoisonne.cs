// RPGArena.CombatEngine/States/Empoisonne.cs
using RPGArena.CombatEngine.Characters;
using RPGArena.CombatEngine.Core;
using RPGArena.CombatEngine.Enums;
using ILogger = RPGArena.CombatEngine.Logging.ILogger;

namespace RPGArena.CombatEngine.States;

public class Empoisonne : State
{
    private readonly int _damagePerTurn = 5;

    public override string Name => "Empoisonné";

    public Empoisonne(Character target, ILogger logger) : base(target)
    {
        _logger = logger;
    }

    public override async Task OnStart()
    {
        for (int i = 0; i < Duration; i++)
        {
            if (_cts.Token.IsCancellationRequested)
                return;

            int damage = 5 * StackCount;
            Target.Life -= damage;
            _logger?.Log($"☠️ {Target.Name} subit {damage} points de dégâts à cause du poison. (Stack: {Stack})");
            await Task.Delay(1000, _cts.Token);
        }

        End();
    }



    public override void Stack()
    {
        base.Stack();
        _logger?.Log($"Le poison sur {Target.Name} est renforcé. Stack = {Stack}");
    }

    public override void End()
    {
        _logger?.Log($"Le poison de {Target.Name} s'est dissipé.");
        base.End();
    }
}