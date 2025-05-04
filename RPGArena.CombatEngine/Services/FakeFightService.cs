using RPGArena.CombatEngine.Core;
using RPGArena.CombatEngine.Enums;
using RPGArena.CombatEngine.Interface;

namespace RPGArena.CombatEngine.Services
{
    public class FakeFightService : IFightService
    {
        public int FixedDamage { get; set; } = 10;
        public bool ForceCritique { get; set; } = false;
        public bool ForceContre { get; set; } = false;

        public int CalculateDamage(Character attacker, Character defender, TypeAttack typeAttack, ResultDe resultAttack, ResultDe resultDefense)
        {
            if (ForceContre)
            {
                return defender.Attack * 2; // simulate une contre-attaque
            }

            if (ForceCritique)
            {
                return attacker.Attack * 2;
            }

            return FixedDamage;
        }
    }
}
