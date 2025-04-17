using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RPGArena.CombatEngine.Core;
using RPGArena.CombatEngine.Interface;
using RPGArena.CombatEngine.Observeur;
using RPGArena.CombatEngine.State;

namespace RPGArena.CombatEngine.Decorateur
{
    internal class EstMange : EtatDecorateurAsync
    {
        private readonly object IsEatableLock = new object();
        public override bool IsEatable
        {
            get { return false; } // le personnage mort est mangé et n'est plus mangeable.
            set { base.IsEatable = value; }
        }
        public EstMange(Character personnage) : base(personnage)
        {
            lock (IsEatableLock)
            {
                base.IsEatable = false; // Mise à jour de la propriété.
                if (!(personnage is IsDead) && IsEatable)
                {
                    Message message = new Message();
                    message.AddSegment($"{personnage.Name} est mangé.");
                    message.AddSegment($"{personnage.Name} n'est plus mangeable.");
                    Character.notify.AddMessageToQueue(message);
                    return;
                }
                else
                {
                    Message message = new Message();
                    message.AddSegment($"quelqu'un essaye de de mangé {personnage.Name} mais il n'est plus mangeable.");
                    Character.notify.AddMessageToQueue(message);
                    return;
                }
            }
            
        }

        public override int Life
        {
            get { return 0; } // Un personnage mangé n'a plus de Life.
            set { } // Ne rien faire, car la Life ne peut pas être modifiée une fois mangée.
        }

        public override void Attack(Character target)
        {
            // Un personnage mangé ne peut pas Attack. 
            // Ne rien faire ou peut-être afficher un message d'erreur.
        }

    }
}
