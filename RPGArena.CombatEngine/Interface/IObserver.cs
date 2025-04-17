using RPGArena.CombatEngine.Observeur;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGArena.CombatEngine.Interface
{
    // Observer interface
    public interface IConsoleObserver
    {
        void Update(ConsoleColor color, string message);
    }

    public interface IMessageObserver
    {
        void Update(Message message);
    }

}
