// RPGArena.CombatEngine/Characters/ICharacterFactory.cs

namespace RPGArena.CombatEngine.Characters
{
    public interface ICharacterFactory
    {
        ICharacter CreateCharacter(string type, string name);
    }
}