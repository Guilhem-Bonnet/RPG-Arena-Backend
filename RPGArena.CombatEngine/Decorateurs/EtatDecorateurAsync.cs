using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RPGArena.CombatEngine.Core;
using RPGArena.CombatEngine.Interface;

namespace RPGArena.CombatEngine.State
{
    public abstract class EtatDecorateurAsync
    {
        protected ICharacter PersonnageDecore;
        

        public virtual bool Mort
        {
            get { return PersonnageDecore.IsDead; } // Par défaut, un personnage est vivant

        }
        public virtual bool IsAttackable
        {
            get { return PersonnageDecore.IsAttackable; } // Par défaut, un personnage est attaquable
        }
        public virtual bool IsEatable
        {
            get { return PersonnageDecore.IsEatable; } // Par défaut, un personnage n'est pas mangeable
            set { PersonnageDecore.IsEatable = value; }
        }

        public EtatDecorateurAsync(ICharacter personnage)
        {
            PersonnageDecore = personnage;
            
        }

        public virtual int Life
        {
            get { return PersonnageDecore.Life; }
            set { PersonnageDecore.Life = value; }
        }

        public virtual int AttackValue
        {
            get { return PersonnageDecore.Attack; }
            set { PersonnageDecore.Attack = value; }
        }

        public virtual int DefenseValue
        {
            get { return PersonnageDecore.Defense; }
            set { PersonnageDecore.Defense = value; }
        }

        public virtual void Attack(Character target)
        {
            PersonnageDecore.AttackBase(target);
        }



        
    }
}
