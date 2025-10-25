using Xunit;
using RPGArena.CombatEngine.Characters;
using RPGArena.CombatEngine.Core;
using RPGArena.CombatEngine.Logging;
using RPGArena.CombatEngine.Services;

namespace RPGArena.Tests;

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
        // Act
        var character = _factory.CreateCharacter(type, name);

        // Assert
        Assert.NotNull(character);
        Assert.Equal(name, character.Name);
        Assert.True(character.Life > 0);
    }

    [Fact]
    public void CreateCharacter_InvalidType_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => _factory.CreateCharacter("InvalidType", "Test"));
    }

    [Fact]
    public void CreateCharacter_CaseInsensitive_Success()
    {
        // Act
        var char1 = _factory.CreateCharacter("GUERRIER", "Test1");
        var char2 = _factory.CreateCharacter("guerrier", "Test2");
        var char3 = _factory.CreateCharacter("Guerrier", "Test3");

        // Assert
        Assert.NotNull(char1);
        Assert.NotNull(char2);
        Assert.NotNull(char3);
    }
}
