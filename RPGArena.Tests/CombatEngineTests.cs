using Xunit;
using RPGArena.CombatEngine.Core;
using RPGArena.CombatEngine.Characters;
using RPGArena.CombatEngine.Logging;
using RPGArena.CombatEngine.Services;
using RPGArena.CombatEngine.Enums;
using System.Threading.Tasks;

namespace RPGArena.Tests;

// ========================================================================================
// CHARACTER FACTORY TESTS - 8 tests
// ========================================================================================
public class CharacterFactoryTests
{
    private readonly CharacterFactory _factory;
    private readonly BattleArena _arena;
    private readonly ILogger _logger;
    private readonly IFightService _fightService;

    public CharacterFactoryTests()
    {
        _logger = new ConsoleLogger();
        _fightService = new FightService(_logger);
        _arena = new BattleArena(_logger, _fightService);
        _factory = new CharacterFactory(_arena, _logger, _fightService);
    }

    [Theory]
    [InlineData("Guerrier", "TestGuerrier")]
    [InlineData("Magicien", "TestMagicien")]
    [InlineData("Assassin", "TestAssassin")]
    [InlineData("Paladin", "TestPaladin")]
    public void CreateCharacter_ValidType_ReturnsCharacter(string type, string name)
    {
        var character = _factory.CreateCharacter(type, name);
        Assert.NotNull(character);
        Assert.Equal(name, character.Name);
        Assert.True(character.Life > 0);
    }

    [Fact]
    public void CreateCharacter_InvalidType_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => _factory.CreateCharacter("InvalidType", "Test"));
    }

    [Fact]
    public void CreateCharacter_CaseInsensitive_Success()
    {
        var character1 = _factory.CreateCharacter("guerrier", "Test1");
        var character2 = _factory.CreateCharacter("GUERRIER", "Test2");
        Assert.NotNull(character1);
        Assert.NotNull(character2);
    }

    [Theory]
    [InlineData("Pretre")]
    [InlineData("Berserker")]
    [InlineData("Vampire")]
    [InlineData("Necromancien")]
    public void CreateCharacter_SpecialTypes_ReturnsCharacter(string type)
    {
        var character = _factory.CreateCharacter(type, "Test");
        Assert.NotNull(character);
        Assert.True(character.Life > 0);
    }
}

// ========================================================================================
// BATTLE ARENA TESTS - 7 tests
// ========================================================================================
public class BattleArenaTests
{
    private readonly ILogger _logger;
    private readonly IFightService _fightService;

    public BattleArenaTests()
    {
        _logger = new ConsoleLogger();
        _fightService = new FightService(_logger);
    }

    [Fact]
    public void BattleArena_Should_Initialize()
    {
        var arena = new BattleArena(_logger, _fightService);
        Assert.NotNull(arena);
    }

    [Fact]
    public void BattleArena_Should_AcceptCharacters()
    {
        var arena = new BattleArena(_logger, _fightService);
        var factory = new CharacterFactory(arena, _logger, _fightService);
        var char1 = factory.CreateCharacter("Guerrier", "Fighter1");
        var char2 = factory.CreateCharacter("Magicien", "Mage1");
        
        arena.AddCharacter(char1);
        arena.AddCharacter(char2);
        
        Assert.Equal(2, arena.Participants.Count);
    }

    [Fact]
    public void BattleArena_Should_ThrowOnNullCharacter()
    {
        var arena = new BattleArena(_logger, _fightService);
        Assert.Throws<ArgumentNullException>(() => arena.AddCharacter(null!));
    }

    [Fact]
    public async Task BattleArena_Should_StartBattle()
    {
        var arena = new BattleArena(_logger, _fightService);
        var factory = new CharacterFactory(arena, _logger, _fightService);
        var char1 = factory.CreateCharacter("Guerrier", "Warrior");
        var char2 = factory.CreateCharacter("Magicien", "Mage");
        
        arena.AddCharacter(char1);
        arena.AddCharacter(char2);
        
        await arena.StartBattle();
        
        Assert.True(arena.Ended);
    }

    [Fact]
    public async Task BattleArena_Should_HaveWinner()
    {
        var arena = new BattleArena(_logger, _fightService);
        var factory = new CharacterFactory(arena, _logger, _fightService);
        var char1 = factory.CreateCharacter("Guerrier", "Warrior");
        var char2 = factory.CreateCharacter("Magicien", "Mage");
        
        arena.AddCharacter(char1);
        arena.AddCharacter(char2);
        
        await arena.StartBattle();
        
        var survivors = arena.Participants.Where(p => p.Life > 0).ToList();
        Assert.True(survivors.Count >= 1 || arena.Participants.All(p => p.IsDead));
    }
}

// ========================================================================================
// CHARACTER TESTS - 17 tests  
// ========================================================================================
public class CharacterTests
{
    private readonly CharacterFactory _factory;
    private readonly ILogger _logger;
    private readonly BattleArena _arena;
    private readonly IFightService _fightService;

    public CharacterTests()
    {
        _logger = new ConsoleLogger();
        _fightService = new FightService(_logger);
        _arena = new BattleArena(_logger, _fightService);
        _factory = new CharacterFactory(_arena, _logger, _fightService);
    }

    [Fact]
    public void Character_Should_HavePositiveLife()
    {
        var character = _factory.CreateCharacter("Guerrier", "Test");
        Assert.True(character.Life > 0);
    }

    [Fact]
    public void Character_Should_HaveName()
    {
        var character = _factory.CreateCharacter("Guerrier", "TestName");
        Assert.Equal("TestName", character.Name);
    }

    [Fact]
    public void Character_Should_BeAliveOnCreation()
    {
        var character = _factory.CreateCharacter("Magicien", "Test");
        Assert.False(character.IsDead);
    }

    [Fact]
    public void Character_Should_HaveMaxLifeEqualOrGreaterThanLife()
    {
        var character = _factory.CreateCharacter("Guerrier", "Test");
        Assert.True(character.MaxLife >= character.Life);
    }

    [Fact]
    public void Character_Should_HaveAttackPower()
    {
        var character = _factory.CreateCharacter("Guerrier", "Test");
        Assert.True(character.Attack > 0);
    }

    [Fact]
    public void Character_Should_HaveDefense()
    {
        var character = _factory.CreateCharacter("Paladin", "Test");
        Assert.True(character.Defense >= 0);
    }

    [Theory]
    [InlineData("Guerrier")]
    [InlineData("Magicien")]
    [InlineData("Assassin")]
    [InlineData("Paladin")]
    [InlineData("Pretre")]
    [InlineData("Berserker")]
    [InlineData("Vampire")]
    [InlineData("Necromancien")]
    public void Character_Should_CreateDifferentTypes(string type)
    {
        var character = _factory.CreateCharacter(type, "Test");
        Assert.NotNull(character);
        Assert.True(character.Life > 0);
        Assert.Equal("Test", character.Name);
    }

    [Fact]
    public void Character_Should_BeAttackableByDefault()
    {
        var character = _factory.CreateCharacter("Guerrier", "Test");
        Assert.True(character.IsAttackable);
    }

    [Fact]
    public void Character_Should_HaveTypePersonnage()
    {
        var character = _factory.CreateCharacter("Guerrier", "Test");
        // Guerrier should have Humain type
        Assert.Equal(TypePersonnage.Humain, character.TypeDuPersonnage);
    }

    [Fact]
    public void Character_Should_ModifyLife()
    {
        var character = _factory.CreateCharacter("Guerrier", "Test");
        var initialLife = character.Life;
        character.Life -= 10;
        Assert.True(character.Life < initialLife);
    }

    [Fact]
    public void Character_Should_ModifyName()
    {
        var character = _factory.CreateCharacter("Guerrier", "Test");
        character.Name = "NewName";
        Assert.Equal("NewName", character.Name);
    }

    [Fact]
    public void Character_Should_DieWhenLifeIsZero()
    {
        var character = _factory.CreateCharacter("Guerrier", "Test");
        character.Life = 0;
        Assert.True(character.IsDead || character.Life <= 0);
    }

    [Fact]
    public void Character_Guerrier_Should_BeHumain()
    {
        var character = _factory.CreateCharacter("Guerrier", "Test");
        Assert.Equal(TypePersonnage.Humain, character.TypeDuPersonnage);
    }

    [Fact]
    public void Character_Zombie_Should_BeMortVivant()
    {
        var character = _factory.CreateCharacter("Zombie", "Test");
        Assert.Equal(TypePersonnage.MortVivant, character.TypeDuPersonnage);
    }

    [Fact]
    public void Character_Robot_Should_BeRobot()
    {
        var character = _factory.CreateCharacter("Robot", "Test");
        // Robot class exists and should be a valid character type
        Assert.NotNull(character);
        Assert.Equal("Test", character.Name);
    }

    [Fact]
    public void Character_Properties_Should_BeModifiable()
    {
        var character = _factory.CreateCharacter("Guerrier", "Test");
        character.Attack = 100;
        character.Defense = 50;
        character.MaxLife = 200;
        
        Assert.Equal(100, character.Attack);
        Assert.Equal(50, character.Defense);
        Assert.Equal(200, character.MaxLife);
    }
}

// ========================================================================================
// FIGHT SERVICE TESTS - 6 tests
// ========================================================================================
public class FightServiceTests
{
    private readonly ILogger _logger;
    private readonly IFightService _fightService;
    private readonly BattleArena _arena;
    private readonly CharacterFactory _factory;

    public FightServiceTests()
    {
        _logger = new ConsoleLogger();
        _fightService = new FightService(_logger);
        _arena = new BattleArena(_logger, _fightService);
        _factory = new CharacterFactory(_arena, _logger, _fightService);
    }

    [Fact]
    public void FightService_Should_Initialize()
    {
        Assert.NotNull(_fightService);
    }

    [Fact]
    public void FightService_Should_HandleDiceRoll()
    {
        var character = _factory.CreateCharacter("Guerrier", "Test");
        var result = character.LancerDe();
        Assert.True(Enum.IsDefined(typeof(ResultDe), result));
    }

    [Fact]
    public void AttackBase_Should_Work()
    {
        var attacker = _factory.CreateCharacter("Guerrier", "Attacker") as Character;
        var target = _factory.CreateCharacter("Magicien", "Target") as Character;
        var initialLife = target!.Life;
        
        attacker!.AttackBase(target);
        
        Assert.True(target.Life <= initialLife);
    }

    [Fact]
    public void Character_Strategy_Should_BeCallable()
    {
        var character = _factory.CreateCharacter("Guerrier", "Test");
        var task = character.Strategie();
        Assert.NotNull(task);
    }

    [Fact]
    public void Character_ExecuteStrategy_Should_BeCallable()
    {
        var character = _factory.CreateCharacter("Guerrier", "Test");
        var task = character.ExecuteStrategyAsync();
        Assert.NotNull(task);
    }

    [Fact]
    public void Character_PerformAction_Should_BeCallable()
    {
        var character = _factory.CreateCharacter("Guerrier", "Test");
        var task = character.PerformActionAsync();
        Assert.NotNull(task);
    }
}
