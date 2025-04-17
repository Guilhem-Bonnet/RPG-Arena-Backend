using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RPGArena.CombatEngine.Core;
using RPGArena.CombatEngine.Interface;
using RPGArena.CombatEngine.State;

namespace RPGArena.CombatEngine.Decorateur
{
    internal class IsDead : EtatDecorateurAsync
    {

        public IsDead(ICharacter personnage) : base(personnage)
        {

        }
        public override bool IsAttackable
        {
            get { return false; } // Un personnage mort n'est pas attaquable
        }
        public override bool IsEatable
        {
            get { return true; } // Un personnage mort est mangeable
            set { base.IsEatable = value; }
        }
        public override bool Mort
        {
            get { return true; } // Un personnage dans l'état "Mort" est effectivement mort
        }

        public override int Life
        {
            get { return 0; } // Un personnage mort n'a plus de Life.
            set { } // Ne rien faire, car la Life ne peut pas être modifiée une fois mort.

        }

        public override void Attack(Character target)
        {
            // Un personnage mort ne peut pas Attack.
            // Ne rien faire ou peut-être afficher un message d'erreur.
        }

    }
}