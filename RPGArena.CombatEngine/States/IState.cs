// RPGArena.CombatEngine/States/IState.cs
using RPGArena.CombatEngine.Core;

namespace RPGArena.CombatEngine.States;

public interface IState
{
    string Name { get; }
    int Duration { get; }
    Task Apply();
    Task OnStart();
    void End();
    void Stack();
    void ResetStack();
}