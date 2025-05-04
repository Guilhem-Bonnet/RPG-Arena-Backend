// RPGArena.CombatEngine/Core/Character.cs
using RPGArena.CombatEngine.Enums;
using RPGArena.CombatEngine.Services;
using RPGArena.CombatEngine.Characters;
using ILogger = RPGArena.CombatEngine.Logging.ILogger;
using RPGArena.CombatEngine.States;
using RPGArena.CombatEngine.Skills;
using RPGArena.CombatEngine.Logging;

namespace RPGArena.CombatEngine.Core;

public abstract class Character : ICharacter
{
    private int _life;
    private bool? _isEatable;
    private readonly object stateLock = new();
    protected readonly Random _rand = new();
    protected readonly IFightService _fightservice;
    protected BattleArena _arena;
    protected readonly ILogger _logger;
    const int FACE_DE = 20;
    

    public string Name { get; set; }
    public int MaxLife { get; protected set; } = 100;
    public int Attack { get; set; } = 10;
    public int Defense { get; set; } = 5;
    public virtual bool CanAct { get; set; } = true;

    public TypePersonnage TypeDuPersonnage { get; protected set; } = TypePersonnage.Humain;

    public virtual bool IsAttackable => !IsDead;
    public virtual bool IsDead => Life <= 0;
    public virtual bool IsEatable
    {
        get => _isEatable ?? IsDead;
        set => _isEatable = value;
    }

    public int Life
    {
        get => _life;
        set => _life = value;
    }
    int ICharacter.MaxLife { get => MaxLife; set => MaxLife = value; }
    TypePersonnage ICharacter.TypeDuPersonnage { get => TypeDuPersonnage; set => TypeDuPersonnage = value; }
    List<IISkill> ICharacter.ISkills { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    protected readonly List<IState> _states = new();
    protected readonly List<ISkill> _skills = new();
    public BattleArena Arena => _arena;


    public Character(string name, BattleArena arena, ILogger logger = null, IFightService fightservice = null)
    {
        Name = name;
        _arena = arena;
        _logger = logger ?? new ConsoleLogger();
        _fightservice = fightservice ?? new NoOpFightService();
        _life = MaxLife;

        // Ajout de l'attaque de base en respectant les dépendances injectées
        _skills.Add(new AttackBase(_fightservice, _logger));
        
        OriginalCharacter = this;
    }
    
    /// <summary>
    /// Si ce personnage est une copie (clone, illusion...), cette propriété référence l'original.
    /// Sinon, elle est égale à lui-même.
    /// </summary>
    public virtual Character OriginalCharacter { get; protected set; }



    public abstract Task PerformActionAsync();

    public virtual async Task ExecuteStrategyAsync()
    {
        while (!IsDead && !_arena.Ended)
        {
            if (CanAct)
                await PerformActionAsync();

            await Task.Delay(500);
        }
    }

    public void BasicAttack(ICharacter target)
    {
        _skills[0].Use(this, (Character)target);
    }

    public ResultDe LancerDe()
    {
        int result = _rand.Next(1, FACE_DE + 1);

        return result switch
        {
            1 => ResultDe.EchecCritique,
            20 => ResultDe.RéussiteCritique,
            <= 5 => ResultDe.Echec,
            >= 16 => ResultDe.Réussite,
            _ => ResultDe.Neutre
        };
    }
    


    public void AddState(IState state)
    {
        _states.Add(state);
        _logger.Log($"{Name} est maintenant affecté par {state.Name}.");
    }

    public void RemoveState(IState state)
    {
        state?.End();        // exécute le nettoyage spécifique à l’état
        _states.Remove(state); // puis le retire de la liste
    }
    public void RemoveState<T>() where T : IState
    {
        var state = _states.OfType<T>().FirstOrDefault();
        if (state != null)
        {
            state.End();        // exécute le nettoyage spécifique à l’état
            _states.Remove(state); // puis le retire de la liste
        }
    }


    public void RemoveAllStates<T>()
    {
        _states.RemoveAll(s => s is T);
    }

    public void ApplyOrStackState(IState state)
    {
        lock (stateLock)
        {
            var existing = _states.FirstOrDefault(e => e.GetType() == state.GetType());
            if (existing != null)
                existing.Stack(); 
            else
                AddState(state);
        }
    }

    public bool HasState<T>() where T : IState
    {
        return _states.OfType<T>().Any();
    }

    public void DiceThrower()
    {
        int result = _rand.Next(1, 7);
        _logger.Log($"{Name} lance un dé et obtient {result}.");
    }

    // Utilitaires de ciblage commun
    protected ICharacter? GetRandomEnemy()
    {
        var enemies = _arena.Participants.Where(p => !p.IsDead && p != this).ToList();
        return enemies.Count > 0 ? enemies[_rand.Next(enemies.Count)] : null;
    }

    protected ICharacter? GetWeakestEnemy()
    {
        return _arena.Participants
            .Where(p => !p.IsDead && p != this)
            .OrderBy(p => p.Life)
            .FirstOrDefault();
    }

    protected ICharacter? GetTargetByType(TypePersonnage type)
    {
        var matches = _arena.Participants
            .Where(p => !p.IsDead && p.TypeDuPersonnage == type)
            .ToList();
        return matches.Count > 0 ? matches[_rand.Next(matches.Count)] : null;
    }

    protected ICharacter? GetAllyWithLowestHealth()
    {
        return _arena.Participants
            .Where(p => !p.IsDead && p != this && p.TypeDuPersonnage == this.TypeDuPersonnage)
            .OrderBy(p => p.Life)
            .FirstOrDefault();
    }

    public abstract void AttackBase(Character target);
    public abstract Task Strategie(); // Stratégie de comportement (fuite attaque etc .. )
}
