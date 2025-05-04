// RPGArena.CombatEngine/Services/NoOpFightService.cs
using RPGArena.CombatEngine.Core;
using RPGArena.CombatEngine.Enums;


namespace RPGArena.CombatEngine.Services
{
    public class NoOpFightService : IFightService
    {
        public int CalculateDamage(Character attacker, Character defender, TypeAttack typeAttack, ResultDe resultAttack, ResultDe resultDefense)
        {
            return 0; // Ne fait rien
        }

    }

}