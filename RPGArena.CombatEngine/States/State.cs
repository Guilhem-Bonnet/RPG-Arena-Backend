

// RPGArena.CombatEngine/States/State.cs
using RPGArena.CombatEngine.Core;
using ILogger = RPGArena.CombatEngine.Logging.ILogger;
namespace RPGArena.CombatEngine.States;
using RPGArena.CombatEngine.Logging;
using System.Threading;


public abstract class State : IState
{
    public Character Target { get; }
    public virtual string Name => "État";
    public int Duration { get; protected set; } = 3;
    public int StackCount { get; protected set; } = 1;

    protected CancellationTokenSource _cts = new();
    protected ILogger? _logger;

    protected State(Character target, ILogger? logger = null)
    {
        Target = target;
        _logger = logger;
    }

    public virtual async Task Apply()
    {
        _logger?.Log($"🌀 {Target.Name} subit l’état : {Name}");
        await OnStart();
    }

    public virtual async Task OnStart()
    {
        for (int i = Duration; i > 0; i--)
        {
            if (_cts.Token.IsCancellationRequested)
                return;

            _logger?.Log($"⏳ {Target.Name} : {Name} actif ({i}s)");
            await Task.Delay(1000, _cts.Token);
        }

        End();
    }

    public virtual void Stack()
    {
        StackCount++;
        _logger?.Log($"➕ {Target.Name} : {Name} est cumulé ({StackCount})");
    }

    public virtual void ResetStack()
    {
        StackCount = 1;
        _logger?.Log($"🔁 {Target.Name} : cumul de {Name} réinitialisé");
    }

    public virtual void End()
    {
        _cts.Cancel();
        ResetStack();
        _logger?.Log($"❌ {Target.Name} : {Name} se dissipe.");
    }
}



