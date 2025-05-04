using System.Threading;
using System.Threading.Tasks;
using RPGArena.CombatEngine.Core;
using RPGArena.CombatEngine.Enums;
using RPGArena.CombatEngine.Logging;
using RPGArena.CombatEngine.Services;
using RPGArena.CombatEngine.Skills;
using ILogger = RPGArena.CombatEngine.Logging.ILogger;

namespace RPGArena.CombatEngine.Characters;

public class Illusion : Illusioniste
{
    private readonly CancellationTokenSource _cts;
    private int _currentLife;

    public Illusion(Character original, BattleArena arena, ILogger logger, IFightService fightService)
        : base(original.Name + " (Illusion)", arena, logger, fightService)
    {
        OriginalCharacter = original;
        Attack = 0;
        Defense = 0;
        MaxLife = 1;
        _currentLife = 1;
        _cts = new CancellationTokenSource();

        _skills.Clear();
        _skills.Add(new AttackBase(fightService, logger)); // Illusion peut attaquer

        AutoDestruction();
    }

    private async void AutoDestruction()
    {
        try
        {
            await Task.Delay(TimeSpan.FromSeconds(5), _cts.Token);
            _logger.Log($"💨 {Name} s'est dissipée après 5 secondes.");
            _currentLife = 0;
        }
        catch (TaskCanceledException)
        {
            // Annulation normale si elle est tuée
        }
    }

    public override async Task PerformActionAsync()
    {
        // Attaque aléatoire d’un ennemi
        var cible = _arena.Participants
            .Where(p => p != this && !p.IsDead && p.IsAttackable)
            .OrderBy(_ => _rand.Next())
            .FirstOrDefault();

        if (cible != null)
        {
            var skill = _skills.FirstOrDefault(s => s.IsReady);
            if (skill != null)
            {
                await skill.Use(this, (Character)cible);
            }
        }

        await Task.Delay(500); // Pause légère
    }

    public override void AttackBase(Character target)
    {
        var baseAttack = _skills.FirstOrDefault(s => s is AttackBase && s.IsReady);
        if (baseAttack != null)
        {
            _ = baseAttack.Use(this, target);
        }
    }

    public new int Life
    {
        get => _currentLife;
        set
        {
            if (value < _currentLife)
            {
                _currentLife = 0;
                _cts.Cancel();
                _logger.Log($"💥 {Name} a été touchée et se dissipe instantanément !");
            }
            else
            {
                _currentLife = value;
            }
        }
    }

    public override bool IsDead => _currentLife <= 0;
    public override bool IsAttackable => true;
}
