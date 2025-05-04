using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RPGArena.CombatEngine.Core;
using RPGArena.CombatEngine.Enums;

namespace RPGArena.CombatEngine.Characters
{
    public interface IISkill
    {
        string Name { get; }
        float DelaiRecharge { get; set; }
        float BaseCooldown { get; set; }
        bool EstDisponible { get; }
        public TypeAttack Type { get; set; }
        Task Use(Character lanceur, Character target);

    }
}
