using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RPGArena.CombatEngine.Core;
using RPGArena.CombatEngine.Enums;

namespace RPGArena.CombatEngine.Characters
{
    internal class Vampire : Character
    {
        public Vampire(string Name) : base(Name)
        {
            TypeDuPersonnage = TypePersonnage.MortVivant;
        }

        public override async Task Strategie()
        {
            throw new NotImplementedException();
        }
    }
}
