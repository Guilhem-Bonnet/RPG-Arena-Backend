// RPGArena.CombatEngine/Characters/ICharacterFactory.cs

namespace RPGArena.CombatEngine.Characters
{
    public interface ICharacterFactory
    {
        /// <summary>
        /// Crée une instance de personnage en fonction de son type et nom.
        /// </summary>
        /// <param name="type">Type du personnage (ex: "zombie", "paladin").</param>
        /// <param name="name">Nom du personnage à instancier.</param>
        /// <returns>Une instance d'un personnage conforme à ICharacter.</returns>
        /// 
        ICharacter CreateCharacter(string type, string name);
    }
}