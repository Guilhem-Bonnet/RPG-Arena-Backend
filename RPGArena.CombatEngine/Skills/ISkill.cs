// RPGArena.CombatEngine/ISkills/ISkill.cs
using RPGArena.CombatEngine.Characters;
using RPGArena.CombatEngine.Core;
using RPGArena.CombatEngine.Enums;

namespace RPGArena.CombatEngine.Skills;

public interface ISkill
{
    string Name { get; }
    TypeAttack Type { get; set; }
    float Cooldown { get; set; }
    float BaseCooldown { get; set; }
    bool IsReady { get; }

    Task Use(ICharacter lanceur, ICharacter cible);
    void ReduceRecharge();
}