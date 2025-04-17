using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RPGArena.CombatEngine.Core;

namespace RPGArena.CombatEngine.Characters
{
    internal class Berserker : Character
    {
        public Berserker(string Name) : base(Name)
        {
        }

        public override async Task Strategie()
        {
            throw new NotImplementedException();
        }
    }
}
