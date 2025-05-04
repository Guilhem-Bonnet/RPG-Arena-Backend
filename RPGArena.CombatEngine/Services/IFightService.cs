// RPGArena.CombatEngine/Services/IFightService.cs
using RPGArena.CombatEngine.Core;
using RPGArena.CombatEngine.Enums;

namespace RPGArena.CombatEngine.Services;

public interface IFightService
{
    int CalculateDamage(Character attacker, Character defender, TypeAttack typeAttack, ResultDe resultAttack, ResultDe resultDefense);
}
