// RPGArena.CombatEngine/ISkills/Skill.cs

using RPGArena.CombatEngine.Characters;
using RPGArena.CombatEngine.Core;
using RPGArena.CombatEngine.Enums;

namespace RPGArena.CombatEngine.Skills;

public abstract class Skill : ISkill
{
    public abstract string Name { get; }
    public virtual TypeAttack Type { get; set; } = TypeAttack.Normal;
    public virtual int ValueDommage { get; set; } = 0;
    public virtual float Cooldown { get; set; } = 0;
    public virtual float BaseCooldown { get; set; } = 1;

    public virtual bool IsReady => Cooldown <= 0;

    public abstract Task Use(Character lanceur, Character cible);

    public Task Use(ICharacter lanceur, ICharacter cible)=> Use((Character)lanceur, (Character)cible);

    public virtual void ReduceRecharge()
    {
        if (Cooldown > 0)
            Cooldown--;
    }
}