namespace RPGArena.CombatEngine.Characters
{
    public class CharacterFactory : ICharacterFactory
    {
        public ICharacter CreateCharacter(string type, string name)
        {
            return type.ToLower() switch
            {
                "alchimiste" => new Alchimiste(name),
                "assassin" => new Assassin(name),
                "berserker" => new Berserker(name),
                "guerrier" => new Guerrier(name),
                "illusioniste" => new Illusioniste(name),
                "magicien" => new Magicien(name),
                "necromancien" => new Necromancien(name),
                "paladin" => new Paladin(name),
                "pretre" => new Pretre(name),
                "robot" => new Robot(name),
                "vampire" => new Vampire(name),
                "zombie" => new Zombie(name),
                _ => throw new ArgumentException($"Type de personnage inconnu : {type}")
            };
        }
    }
}
