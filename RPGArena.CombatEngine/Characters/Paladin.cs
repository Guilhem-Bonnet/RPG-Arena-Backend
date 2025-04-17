using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RPGArena.CombatEngine.Core;
using RPGArena.CombatEngine.Enums;

namespace RPGArena.CombatEngine.Characters
{
    internal class Paladin : Character
    {
        public Paladin(string Name) : base(Name)
        {
            _skills[0].Type = TypeAttack.Sacre;
        }

        public override async Task Strategie()
        {
            throw new NotImplementedException();
        }
    }
}
