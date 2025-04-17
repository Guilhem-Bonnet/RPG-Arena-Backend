using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RPGArena.CombatEngine.Characters;
using RPGArena.CombatEngine.Core;
using RPGArena.CombatEngine.Interface;

namespace RPGArena.CombatEngine.State
{
    internal class EtatEtourdi : EtatDecorateurAsync
    {
        private int DureeEtourdissement; // Durée en millisecondes
        private bool EstActuellementEtourdi = true;
        public EtatEtourdi(ICharacter Personnage, int dureeEtourdissement = 5) : base(Personnage)
        {
            this.DureeEtourdissement = dureeEtourdissement;
            InitialiserEtourdissement();
        }

        private async void InitialiserEtourdissement()
        {
            await Task.Delay(DureeEtourdissement);
            EstActuellementEtourdi = false;
        }

        public override void Attack(Character target)
        {
            if (!EstActuellementEtourdi)
            {
                base.Attack(target);
            }
            // Si le personnage est étourdi, il ne fait rien
        }


    }
}
