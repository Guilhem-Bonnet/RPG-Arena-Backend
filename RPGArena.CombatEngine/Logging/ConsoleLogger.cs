using System;
using RPGArena.CombatEngine.Logging;

/*
 <summary>
 Logger de base destiné à afficher les messages du moteur de combat dans la console système.
 Utilisé principalement en mode debug local ou test unitaire.
</summary>
*/ 
namespace RPGArena.CombatEngine.Logging
{
    public class ConsoleLogger : ILogger
    {
        public void Log(string message)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine($"[LOG] {message}");
            Console.ResetColor();
        }
    }
}
