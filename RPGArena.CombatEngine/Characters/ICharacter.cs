using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RPGArena.CombatEngine.Core;
using RPGArena.CombatEngine.Enums;

namespace RPGArena.CombatEngine.Characters 
{ 
    public interface ICharacter
    {
        string Name { get; set; }
        int Life { get; set; }
        int MaxLife { get; set; }
        int Attack { get; set; }
        int Defense { get; set; }
        bool IsAttackable { get; }   // Pour déterminer si le personnage est attaquable
        bool IsDead { get; }         // Pour déterminer si le personnage est mort
        bool IsEatable { get; set; }    // Pour déterminer si le personnage est mangeable
        TypePersonnage TypeDuPersonnage { get; set; }
        void AttackBase(Character target);
        Task Strategie();
        Task ExecuteStrategy();
        List<IISkill> ISkills { get; set; }
        ResultDe LancerDe();
    }
}
